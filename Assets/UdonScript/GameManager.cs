﻿using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;



[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class GameManager : UdonSharpBehaviour
{
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public TableManager TableManager;
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public bool testMode;
    [SerializeField] public JoinStatus JoinStatus;
    [SerializeField] public ResultViewer ResultViewer;

    [SerializeField] public EventLogHandler EventLogHandler;
    [SerializeField] public EventLogger EventLogger;
    [SerializeField] public EventLog EventLog;

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(Seed))] public int seed = 0;

    const string State_WaitForStart = "WaitForStart";
    const string State_WaitForDiscard = "WaitForDiscard";
    const string State_WaitForNaki = "WaitForNaki";
    const string State_EndOfRound = "EndOfRound";
    const string State_EndOfGame = "EndOfGame";

    private bool ReadyForGame = false;
    private bool isNetworkReady;

    private int registeredPlayerCount = 0;
    private int uiActivedCount = 0;
    private string gameState = "";
    private Card waitingNakiCard;
    private float pauseQueueTime = 0.0f;

    private VRCPlayerApi[] registeredPlayers;
    private string[] winds;

    public int Seed
    {
        set
        {
            seed = value;

            Initialize_Local();

            RequestSerialization();
        }

        get => seed;
    }

    void Start()
    {
#if UNITY_EDITOR
        // 로컬 테스트 환경일 때 
        Initialize_Master();
        Initialize_Local();
        ActiveTestMode();
#endif
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        // Master가 처음 들어왔을 때
        if (Networking.IsMaster && player.playerId == Networking.LocalPlayer.playerId)
        {
            Networking.SetOwner(player, gameObject);

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
            //EventLogHandler.Run();
            //Initialize_Local();
        }
    }

    public void Initialize_Master() 
    {
        if(Seed == 0)
            Seed = UnityEngine.Random.Range(1, 2147483647);
        LogViewer.Log($"Create Seed : {Seed}", 0);

        EventLog.SetEvent($"FI&{Seed}");
    }

    public void Initialize_Local()
    {
        UnityEngine.Random.InitState(Seed);
        LogViewer.Log($"Set Seed : {Seed}", 0);
        ChangeGameState(State_WaitForStart);
        TableManager.ResetTable();
        EventQueue.Initialize();
        registeredPlayers = new VRCPlayerApi[4];
        winds = new string[] { "East", "South", "West", "North" };

        ReadyForGame = true;
        LogViewer.Log("Game Initalized", 0);
    }

    void ActiveTestMode()
    {
        // 원래 4명 다 모여야 카드를 배분하지만
        // 유니티에서 혼자 테스트할 용도로 카드 주고 버리기 대기하게 함
        LogViewer.Log("TestMode ON v1.9", 0);
        TableManager.AddNextCard();
        ChangeGameState(State_WaitForDiscard);
        LogViewer.Log("TestMode GameStart", 0);
    }

    void Update()
    {
        if (!Networking.IsNetworkSettled) { return; }

        if (!IsReady()) { return; }

        if (!ReadyForGame && Seed != 0) { 
            Initialize_Local();
            if (testMode)
            {
                ActiveTestMode();
            }
            return;
        }

        if (!ReadyForGame) { return; }

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
                LogViewer.Log($"DrawReason: {inputEvent.DrawReason}", 0);
                TableManager.TableViewer.activeDisplay("Draw", true);


                switch (inputEvent.DrawReason)
                {
                    case "ByFourKKan":
                    case "ByYamaExhausted":
                        // 텐파이 상태의 사람들, 아닌 사람들 체크해서 나눠주기
                        // 유국만관 체크하기
                        // 그다음 뭐 하지?
                        break;
                }

                // 뭔가 더 처리를 해야 하는데 나중에 함
            }

            var requestPlayer = TableManager.GetPlayer(inputEvent.PlayerIndex);

            switch (inputEvent.EventType)
            {
                case "TsumoCut":
                    {
                        requestPlayer.playerStatus.isAutoDiscardMode = !requestPlayer.playerStatus.isAutoDiscardMode;
                        return; return;
                    }

                case "AutoAgari":
                    {
                        requestPlayer.playerStatus.isAutoAgariMode = !requestPlayer.playerStatus.isAutoAgariMode;
                        return;
                    }

                case "NoNaki":
                    {
                        requestPlayer.playerStatus.isNoNakiMode = !requestPlayer.playerStatus.isNoNakiMode;
                        return;
                    }
                case "NotSort":
                    {
                        requestPlayer.playerStatus.isNoSortMode = !requestPlayer.playerStatus.isNoSortMode;
                        return;
                    }
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

    void Draw(InputEvent inputEvent)
    {

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
        var firstTurnIndex = UnityEngine.Random.Range(0, 4);

        for (var i = 0; i < 4; ++i)
        {
            var index = (firstTurnIndex + i) % 4;
            var player = TableManager.GetPlayer(index);
            if(registeredPlayers[index] != null) player.SetPlayerName(registeredPlayers[index].displayName);
            else player.SetPlayerName($"Player {index}");
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
            currentPlayer.OpenHandCards();
            TableManager.TableViewer.activeDisplay("Tsumo", true);
            var playerStatus = currentPlayer.CalculateTsumoScore();

            var TsumoScore = TableManager.HandCalculator.ScoreCalculator.GetScore(playerStatus);

            for (var i =0; i<4; i++)
            {
                var player = TableManager.GetPlayer(i);

                { // 점수적용
                    if (player.PlayerIndex == currentPlayer.PlayerIndex)
                        currentPlayer.AddScore(TsumoScore);
                    else
                        currentPlayer.AddScore(currentPlayer.PlayerIndex == 0 ? TsumoScore / 2 * -1 : TsumoScore / 4 * -1);
                }
            }

            
            

            // 해야 한다...
            var yakuKeyList = playerStatus.YakuKey;
            var hanList = playerStatus.Han;
            var fu = playerStatus.Fu;
            var count = playerStatus.YakuCount;

            Debug.Log($"[GameManager] Tsumo Status \nYaku : {yakuKeyList.ToString()}, \nHan : {hanList.ToString()}, \nFu : {fu}");

            ResultViewer.setResult("쯔모", currentPlayer.PlayerName, count, yakuKeyList, hanList, fu, TsumoScore);

            EventLogger.DeleteEventLogs();

            ChangeGameState(State_EndOfRound);
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

            currentPlayer.SetColliderActive(false, false);
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

            if(TableManager.currentDorasCardIndex >= 4)
            {
                ChangeGameState(State_WaitForNaki);
            }
        }
    }

    void WaitForNaki(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;

        TableManager.GetCurrentTurnPlayer().SetColliderActive(false, true);

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
                    ChangeGameState(State_WaitForDiscard);
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
                    ChangeGameState(State_WaitForDiscard);
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
                    ChangeGameState(State_WaitForDiscard);
                    break;
                }

            case "Ron":
                {
                    TableManager.SetSubtitleAllPlayers(nakiPlayer.gameObject.name, "론!");
                    nakiPlayer.OpenHandCards();
                    TableManager.TableViewer.activeDisplay("Ron", true);
                    var playerStatus = nakiPlayer.CalculateRonScore();

                    var RonScore = TableManager.HandCalculator.ScoreCalculator.GetScore(playerStatus);

                    nakiPlayer.AddScore(RonScore);
                    formerPlayer.AddScore(RonScore * -1);

                    // 해야 한다...
                    var yakuKeyList = playerStatus.YakuKey;
                    var hanList = playerStatus.Han;
                    var fu = playerStatus.Fu;
                    var count = playerStatus.YakuCount;

                    ResultViewer.setResult("론", nakiPlayer.PlayerName, count, yakuKeyList, hanList, fu, RonScore);

                    EventLogger.DeleteEventLogs();

                    ChangeGameState(State_EndOfRound);
                    break;
                }
        }

        uiActivedCount = 0;
        waitingNakiCard = null;
        TableManager.DisableOneShotRiichiAll(); // 누군가 울면 일발이 깨진다 
        TableManager.DisableUIAll();
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
        //지금은 끝나면 바로 초기화하게 해뒀지만 나중엔 버튼을 누르면 초기화 하게 해야함
        TableManager.ResetTable();
        var lastRoundWInd = TableManager.GetPlayer(0).playerStatus.RoundWind;
        TableManager.SetNextRoundWind(lastRoundWInd);
        ChangeGameState(State_WaitForDiscard);
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
            var picked = UnityEngine.Random.Range(0, yetShuffledCount + 1);
            shuffled[shuffledIndex] = players[picked];
            players[picked] = players[yetShuffledCount];

            yetShuffledCount--;
            shuffledIndex++;
        }
        return shuffled;
    }
}
