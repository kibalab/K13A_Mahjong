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

    [UdonSynced(UdonSyncMode.None)] public int Turn = 0;
    [UdonSynced(UdonSyncMode.None)] public string GameState = "";
    [UdonSynced(UdonSyncMode.None)] public int UIActivedCount = 0;
    [UdonSynced(UdonSyncMode.None)] public int RegisteredPlayerCount = 0;

    const string State_WaitForStart = "WaitForStart";
    const string State_WaitForDiscard = "WaitForDiscard";
    const string State_WaitForNaki = "WaitForNaki";
    const string State_EndOfRound = "EndOfRound";
    const string State_EndOfGame = "EndOfGame";

    private Card WaitingNakiCard;
    private float WaitingTime = 0.0f;
    private bool isRunOnMasterScript = false;

    public bool testMode;
    public LogViewer LogViewer;

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

        }
        // Player가 처음 들어왔을 때
        else if (player.playerId == Networking.LocalPlayer.playerId)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "_SyncRequested");
        }
    }

    public void _SyncRequested()
    {
        if (isRunOnMasterScript) { TableManager.SyncCards(); }
    }

    public void Initialize_Master() 
    {
        ChangeGameState(State_WaitForStart);
        TableManager.Initialize();

        isRunOnMasterScript = true;
        LogViewer.Log("Master Initalized", 0);
    }

    void ActiveTestMode()
    {
        // 원래 4명 다 모여야 카드를 배분하지만
        // 유니티에서 혼자 테스트할 용도로 카드 주고 버리기 대기하게 함
        if (testMode)
        {
            LogViewer.Log("TestMode ON", 0);
            TableManager.AddNextCard();
            ChangeGameState(State_WaitForDiscard);
            LogViewer.Log("TestMode GameStart", 0);
        }
    }

    void Update()
    {
        if (!isRunOnMasterScript) { return; }

        if (!EventQueue.IsQueueEmpty())
        {
            var inputEvent = EventQueue.Dequeue();
            LogViewer.Log($"inputEvent ({inputEvent.EventType}, {inputEvent.PlayerIndex}", 0);

            switch (GameState)
            {
                case State_WaitForStart:
                    WaitForStart(inputEvent);
                    break;

                case State_WaitForDiscard:
                    WaitForDiscard(inputEvent);
                    break;

                case State_WaitForNaki:
                    WaitForNaki(inputEvent);
                    break;

                case State_EndOfRound:
                    EndOfGame();
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

            inputEvent.Clear();
        }
    }

    void WaitForStart(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;
        if (eventType == "Register")
        {
            ++RegisteredPlayerCount;
        }

        if (RegisteredPlayerCount == 4 || testMode)
        {
            // 4명 중 아무나 첫 턴으로 설정해준다
            TableManager.SetTurnOf(Random.Range(0, 4));
            TableManager.AddNextCard();

            ChangeGameState(State_WaitForDiscard);
        }
    }

    void WaitForDiscard(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;
        var eventCard = TableManager.GetCardByIndex(inputEvent.DiscardedCardYamaIndex);

        if (!TableManager.IsCurrentTurn(inputEvent.PlayerIndex))
        {
            return;
        }

        if (eventType == "Discard")
        {
            var currentTable = TableManager.GetCurrentTurnPlayer();
            currentTable.Discard(eventCard);

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
                WaitingNakiCard = eventCard;
                UIActivedCount = uiActived;

                LogViewer.Log($"UIActived. Count:{UIActivedCount}", 0);

                ChangeGameState(State_WaitForNaki);
            }
        }
    }

    void WaitForNaki(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;

        if (WaitingNakiCard == null) { Debug.Log("이게 null이면.. 안되는데?"); }

        if (eventType == "Skip" && UIActivedCount > 0)
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
        --UIActivedCount;
        if (UIActivedCount == 0)
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
        formerPlayer.RemoveStashedCard(WaitingNakiCard);
        var nakiPlayer = TableManager.GetPlayer(inputEvent.PlayerIndex);

        switch (inputEvent.EventType)
        {
            case "Chi":
                {
                    var chiCards = new Card[]
                    {
                        WaitingNakiCard,
                        TableManager.GetCardByIndex((int)inputEvent.ChiIndex.x),
                        TableManager.GetCardByIndex((int)inputEvent.ChiIndex.y)
                    };
                    nakiPlayer.OpenCards(chiCards);
                    TableManager.SetTurnOf(inputEvent.PlayerIndex);
                    break;
                }

            case "Pon":
                {
                    var sameOrderCards = nakiPlayer.FindCardByGlobalOrder(WaitingNakiCard.GlobalOrder, 2);
                    var ponCards = new Card[]
                    {
                        WaitingNakiCard,
                        sameOrderCards[0],
                        sameOrderCards[1]
                    };

                    nakiPlayer.OpenCards(ponCards);
                    TableManager.SetTurnOf(inputEvent.PlayerIndex);
                    break;
                }

            case "Kkan":
                {
                    var sameOrderCards = nakiPlayer.FindCardByGlobalOrder(WaitingNakiCard.GlobalOrder, 3);
                    var kkanCards = new Card[]
                    {
                        WaitingNakiCard,
                        sameOrderCards[0],
                        sameOrderCards[1],
                        sameOrderCards[2]
                    };

                    nakiPlayer.OpenCards(kkanCards);
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

        UIActivedCount = 0;
        WaitingNakiCard = null;
        TableManager.DisableUIAll();
        ChangeGameState(State_WaitForDiscard);
    }

    void EndOfGame()
    {
        WaitingTime -= Time.deltaTime;
        if (WaitingTime < 0)
        {
            // 다음 라운드를 시작하는 처리
        }
    }


    void ChangeGameState(string state)
    {
        // state 바뀔 때 로그 띄워주려고 단순 대입이지만 함수로 뺌
        Debug.Log($"GameState = {state}");

        GameState = state;

        // State가 바뀔 때 초기 동작을 여기에 정의
        switch (GameState)
        {
            case State_EndOfRound:
                // 5초동안 결과화면을 보여주고 넘어간다고 하자 (시간은 바뀔 수 있음)
                WaitingTime = 5f;
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
}
