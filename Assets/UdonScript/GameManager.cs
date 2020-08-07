using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class GameManager : UdonSharpBehaviour
{
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public TableManager TableManager;
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public bool testMode;
    [SerializeField] public JoinStatus JoinStatus;

    const string State_WaitForStart = "WaitForStart";
    const string State_WaitForDiscard = "WaitForDiscard";
    const string State_WaitForNaki = "WaitForNaki";
    const string State_EndOfRound = "EndOfRound";
    const string State_EndOfGame = "EndOfGame";

    private bool isRunOnMasterScript = false;
    private bool isNetworkReady;

    private int registeredPlayerCount = 0;
    private int uiActivedCount = 0;
    private string gameState = "";
    private Card waitingNakiCard;
    private float pauseQueueTime = 0.0f;

    private VRCPlayerApi[] registeredPlayers;
    private string[] winds;

    void Start()
    {
        // 로컬 테스트 환경일 때 
        if (Networking.LocalPlayer == null)
        {
            Initialize_Master();

            if (testMode)
            {
                ActiveTestMode();
            }
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // Master가 처음 들어왔을 때
        if (Networking.IsMaster && player.playerId == Networking.LocalPlayer.playerId)
        {
            Initialize_Master();
            if (testMode)
            {
                ActiveTestMode();
            }
        }
        // Master가 다른 사람이 들어온 것을 감지했을 때
        else if (Networking.IsMaster && player.playerId != Networking.LocalPlayer.playerId)
        {
            LogViewer.Log($"Player Joined. {player.displayName}", 0);
        }
        // Player가 처음 들어왔을 때
        else if (player.playerId == Networking.LocalPlayer.playerId)
        {
            LogViewer.Log($"Player Joined. {player.displayName}", 1);
        }
    }

    public void Initialize_Master() 
    {
        ChangeGameState(State_WaitForStart);
        TableManager.Initialize();
        EventQueue.Initialize();
        registeredPlayers = new VRCPlayerApi[4];
        winds = new string[] { "East", "North", "West", "South" };

        isRunOnMasterScript = true;
        LogViewer.Log("Master Initalized", 0);
    }

    void ActiveTestMode()
    {
        // 원래 4명 다 모여야 카드를 배분하지만
        // 유니티에서 혼자 테스트할 용도로 카드 주고 버리기 대기하게 함
        if (testMode)
        {
            LogViewer.Log("TestMode ON v1.9", 0);
            TableManager.AddNextCard();
            ChangeGameState(State_WaitForDiscard);
            LogViewer.Log("TestMode GameStart", 0);
        }
    }

    void Update()
    {
        if (!IsReady()) { return; }

        if (!isRunOnMasterScript) { return; }

        if (pauseQueueTime > 0.0f)
        {
            pauseQueueTime -= Time.deltaTime;
            return;
        }

        if (!EventQueue.IsQueueEmpty())
        {
            var inputEvent = EventQueue.Dequeue();
            LogViewer.Log($"inputEvent ({inputEvent.EventType}, {inputEvent.PlayerIndex})", 0);

            if (inputEvent.EventType == "Draw") // 유국
            {
                Debug.Log($"DrawReason: {inputEvent.DrawReason}");
                // 뭔가 더 처리를 해야 하는데 나중에 함
            }
            
            // 액션 이전의 UI를 전부 disable함
            // 안 해주면 이전 UI가 남아있다
            TableManager.DisableUIAll();

            switch (gameState)
            {
                case State_WaitForStart:
                    WaitForStart(inputEvent);
                    break;

                case State_WaitForDiscard:
                    WaitForPlayerAction(inputEvent);
                    break;

                case State_WaitForNaki:
                    WaitForNaki(inputEvent);
                    break;

                case State_EndOfRound:
                    EndOfRound();
                    // 역에 대한 결과를 보여주는 단계
                    // 만들어야 함 (UI 디자인부터 해야...)
                    break;

                case State_EndOfGame:
                    // 전체 스코어를 보여주는 단계
                    // 만들어야 함 (UI 디자인부터 해야...)
                    break;

                default:
                    break;
            }
        }
    }

    void WaitForStart(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;
        var player = inputEvent.NewPlayer;
        if (eventType == "Register")
        {
            registeredPlayers[registeredPlayerCount++] = player;
            LogViewer.Log($"[GameManager] Registering Player : {player.displayName}", 0);
        }

        LogViewer.Log($"[GameManager] registeredPlayersCount : {registeredPlayerCount}", 1);
        var joinedNetworkMessage = "";
        for (var i = 0; i < registeredPlayerCount; ++i)
        {
            var p = registeredPlayers[i];
            joinedNetworkMessage += $"{p.displayName}[Joined],";
        }

        LogViewer.Log($"[GameManager] Send JoinStatus Data : {joinedNetworkMessage}", 1);
        JoinStatus.setNetworkMessage(joinedNetworkMessage);

        if (registeredPlayerCount == 4 || testMode)
        {
            StartGame();
        }
    }

    void StartGame()
    {
        // 등록한 순서를 적당히 섞는다
        registeredPlayers = ShufflePlayers(registeredPlayers);

        // 4명 중 아무나 첫 턴으로 설정해준다
        var firstTurnIndex = Random.Range(0, 4);

        for (var i = 0; i < 4; ++i)
        {
            var index = (firstTurnIndex + i) % 4;
            var player = TableManager.GetPlayer(index);
            player.SetPlayerName(registeredPlayers[index].displayName);
            player.SetWind(winds[index]); // 방위 설정
        }

        TableManager.SetTurnOf(firstTurnIndex);
        // 첫 판이 동풍전 맞던가... 
        TableManager.SetRoundWind("East"); 
        TableManager.AddNextCard();

        ChangeGameState(State_WaitForDiscard);
    }


    void WaitForPlayerAction(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;

        if (!TableManager.IsCurrentTurn(inputEvent.PlayerIndex))
        {
            return;
        }

        var currentPlayer = TableManager.GetCurrentTurnPlayer();

        if (eventType == "Kkan")
        {
            var ankkanableGlobalOrders = currentPlayer.FindAnkkanableGlobalOrders();
            TableManager.SetSubtitleAllPlayers(currentPlayer.gameObject.name, "깡");
            // 지금은 안깡, 소명깡이 겹칠 때 안깡부터 하게 한다
            // 선택이 생긴다면... InputEvent에 SelectedChiIndex처럼 
            // SelectedKkanGlobalOrder 같은 걸 설정해줘야 할거임
            // UIContext에도 KkanGlobalOrder 1,2,3,4,5 생겨야할거고

            if (ankkanableGlobalOrders.Length != 0)
            {
                // 여러개 할 수 있지만 일단은 첫번째 GlobalOrder 씀
                var ankkanTargetGlobalOrder = ankkanableGlobalOrders[0];
                var sameOrderCards = currentPlayer.FindCardByGlobalOrder(ankkanTargetGlobalOrder, 4);
                var ankkanCards = new Card[]
                {
                    sameOrderCards[0],
                    sameOrderCards[1],
                    sameOrderCards[2],
                    sameOrderCards[3]
                };

                currentPlayer.OpenCards(ankkanCards, getPlayerDirection(TableManager.currentTurnPlayer, currentPlayer.PlayerIndex));

                TableManager.SetTurnOf(inputEvent.PlayerIndex);
                TableManager.AddNextRinShanCard();
            }
            else // 소명깡
            {
                currentPlayer.AddOpenKkan();
            }

            currentPlayer.AddKkanCount();
            ChangeGameState(State_WaitForDiscard);
        }
        else if (eventType == "Skip")
        {
            currentPlayer.DisableUI();
        }
        else if (eventType == "Rich") // <- 이걸 고치려면 버튼의 물리적 이름을 Riichi로 바꿔야 한다
        {
            TableManager.SetSubtitleAllPlayers(currentPlayer.gameObject.name, "리치!");
            // 리치 관련 카드만 콜라이더를 킨다
            currentPlayer.ActiveRiichiCreateCardColliders();
            currentPlayer.DisableUI();
        }
        else if (eventType == "Tsumo")
        {
            TableManager.SetSubtitleAllPlayers(currentPlayer.gameObject.name, "쯔모");
            var playerStatus = currentPlayer.CalculateTsumoScore();

            var yakuKeyList = playerStatus.YakuKey;
            var hanList = playerStatus.Han;
            var fu = playerStatus.Fu;
            var count = playerStatus.YakuCount;
            // 해야 한다...
        }
        else if (eventType == "AutoDiscard")
        {
            pauseQueueTime = 3.0f;

            // 3초 대기하고 일반적인 discard로 이동 
            EventQueue.SetDiscardEvent(inputEvent.DiscardedCardYamaIndex, inputEvent.PlayerIndex);
        }
        else if (eventType == "RiichiDiscard")
        {
            // 리치봉 놓은 다음 점수 깎고 패 가로로 돌려놓는 처리 해야 함
            //  - ActiveRiichiMode() 에서 처리하게 해둠
            currentPlayer.ActiveRiichiMode();


            // 리치 관련 처리 하고 일반적인 Discard로 이동
            EventQueue.SetDiscardEvent(inputEvent.DiscardedCardYamaIndex, inputEvent.PlayerIndex);

            currentPlayer.SetColliderActive(false);
        }
        else if (eventType == "Discard")
        {
            var eventCard = TableManager.GetCardByIndex(inputEvent.DiscardedCardYamaIndex);
            currentPlayer.DisableUI();
            currentPlayer.Discard(eventCard);

            TableManager.AnnounceDiscard(eventCard);

            var uiActived = TableManager.GetUIActivedUserCount();
            if (uiActived == 0)
            {
                TableManager.SetNextTurn();
                TableManager.AddNextCard();

                ChangeGameState(State_WaitForDiscard);
            }
            else
            {
                waitingNakiCard = eventCard;
                uiActivedCount = uiActived;

                LogViewer.Log($"UIActived. Count:{uiActivedCount}", 0);

                ChangeGameState(State_WaitForNaki);
            }

        }
    }

    void WaitForNaki(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;

        if (waitingNakiCard == null) { Debug.Log("이게 null이면.. 안되는데?"); }

        if (eventType == "Skip" && uiActivedCount > 0)
        {
            ProcessSkip();
        }
        else if (IsNakiInput(inputEvent))
        {
            ProcessNaki(inputEvent);
        }
    }

    void ProcessSkip()
    {
        --uiActivedCount;
        if (uiActivedCount == 0)
        {
            TableManager.DisableUIAll();
            TableManager.SetNextTurn();
            TableManager.AddNextCard();
            ChangeGameState(State_WaitForDiscard);
        }
    }

    void ProcessNaki(InputEvent inputEvent)
    {
        var formerPlayer = TableManager.GetCurrentTurnPlayer();
        formerPlayer.RemoveStashedCard(waitingNakiCard);
        var nakiPlayer = TableManager.GetPlayer(inputEvent.PlayerIndex);

        switch (inputEvent.EventType)
        {
            case "Chi":
                {
                    TableManager.SetSubtitleAllPlayers(nakiPlayer.gameObject.name, "치!");
                    var chiCards = new Card[]
                    {
                        waitingNakiCard,
                        TableManager.GetCardByIndex((int)inputEvent.ChiIndex.x),
                        TableManager.GetCardByIndex((int)inputEvent.ChiIndex.y)
                    };

                    // 0:오른쪽. 1:반대편, 2:왼쪽, 3:안깡
                    nakiPlayer.OpenCards(chiCards, 2); 
                    TableManager.SetTurnOf(inputEvent.PlayerIndex);
                    break;
                }

            case "Pon":
                {
                    TableManager.SetSubtitleAllPlayers(nakiPlayer.gameObject.name, "퐁!");
                    var sameOrderCards = nakiPlayer.FindCardByGlobalOrder(waitingNakiCard.GlobalOrder, 2);
                    var ponCards = new Card[]
                    {
                        waitingNakiCard,
                        sameOrderCards[0],
                        sameOrderCards[1]
                    };

                    nakiPlayer.OpenCards_Pon(ponCards, getPlayerDirection(TableManager.currentTurnPlayer,nakiPlayer.PlayerIndex));
                    TableManager.SetTurnOf(inputEvent.PlayerIndex);
                    break; 
                }

            case "Kkan":
                {
                    TableManager.SetSubtitleAllPlayers(nakiPlayer.gameObject.name, "깡!");
                    var sameOrderCards = nakiPlayer.FindCardByGlobalOrder(waitingNakiCard.GlobalOrder, 3);
                    var kkanCards = new Card[]
                    {
                        waitingNakiCard,
                        sameOrderCards[0],
                        sameOrderCards[1],
                        sameOrderCards[2]
                    };

                    nakiPlayer.OpenCards(kkanCards, getPlayerDirection(TableManager.currentTurnPlayer, nakiPlayer.PlayerIndex));
                    TableManager.SetTurnOf(inputEvent.PlayerIndex);
                    TableManager.AddNextRinShanCard();
                    break;
                }

            case "Ron":
                {
                    // 해야한다..
                    break;
                }
        }

        uiActivedCount = 0;
        waitingNakiCard = null;
        TableManager.DisableOneShotRiichiAll(); // 누군가 울면 일발이 깨진다 
        TableManager.DisableUIAll();
        ChangeGameState(State_WaitForDiscard);
    }

    int getPlayerDirection(int currentPlayerIndex, int targetPlayerIndex)
    {
        if(currentPlayerIndex == targetPlayerIndex)
        {
            return 3;
        }
        for(var i = 1; i<=4; i++)
        {
            if((currentPlayerIndex + i) % 4 == targetPlayerIndex)
            {
                return 2 - i - 1 *-1;
            }
        }
        return 0;
    }

    void EndOfRound()
    {
    
    }

    void ChangeGameState(string state)
    {
        // state 바뀔 때 로그 띄워주려고 단순 대입이지만 함수로 뺌
        Debug.Log($"GameState = {state}");

        gameState = state;

        // State가 바뀔 때 초기 동작을 여기에 정의
        switch (gameState)
        {
            case State_EndOfRound:
                // 5초동안 결과화면을 보여주고 넘어간다고 하자 (시간은 바뀔 수 있음)
                pauseQueueTime = 5f;
                break;
        }
    }

    bool IsNakiInput(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;
        return eventType == "Chi"
            || eventType == "Pon"
            || eventType == "Kkan"
            || eventType == "Ron";
    }

    bool IsReady()
    {
        if (isNetworkReady) { return true; }
        isNetworkReady = Networking.IsObjectReady(gameObject) && TableManager.IsReady();
        return isNetworkReady;
    }

    public VRCPlayerApi[] ShufflePlayers(VRCPlayerApi[] players)
    {
        var shuffled = new VRCPlayerApi[4];
        var yetShuffledCount = 4 - 1;
        var shuffledIndex = 0;

        while (yetShuffledCount >= 0)
        {
            var picked = Random.Range(0, yetShuffledCount + 1);
            shuffled[shuffledIndex] = players[picked];
            players[picked] = players[yetShuffledCount];

            yetShuffledCount--;
            shuffledIndex++;
        }
        return shuffled;
    }
}
