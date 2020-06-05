using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GameManager : UdonSharpBehaviour
{
    /*LinkedInInspector*/ public EventQueue EventQueue;
    /*LinkedInInspector*/ public TableManager TableManager;

    [UdonSynced(UdonSyncMode.None)] public int Turn = 0;
    [UdonSynced(UdonSyncMode.None)] public string GameState = "";
    [UdonSynced(UdonSyncMode.None)] public int UIActivedCount = 0;
    [UdonSynced(UdonSyncMode.None)] public int RegisteredPlayerCount = 0;

    const string State_WaitForStart = "WaitForStart";
    const string State_WaitForDiscard = "WaitForDiscard";
    const string State_WaitForNaki = "WaitForNaki";
    const string State_StartNextRound = "StartNextRound";
    const string State_GameEnd = "GameEnd";

    private Card WaitingNakiCard;

    void Start()
    {
        if (Networking.GetOwner(this.gameObject) == null)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

        TableManager.Initialize();
        ChangeGameState(State_WaitForStart);

        if (Networking.LocalPlayer == null)
        {
            SettingForUnityTests();
        }
    }

    void SettingForUnityTests()
    {
        TableManager.AddNextCard();
        ChangeGameState(State_WaitForDiscard);
    }

    void Update()
    {
        if (!EventQueue.IsQueueEmpty())
        {
            var inputEvent = EventQueue.Dequeue();
            Debug.Log($"inputEvent ({inputEvent.EventType}, {inputEvent.PlayerIndex}, {inputEvent.CardIndex})");

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

                case State_StartNextRound:
                    // 만들어야 한다...
                    break;

                case State_GameEnd:
                    // 만들어야 한다...
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

        if (RegisteredPlayerCount == 4)
        {
            // 4명 중 아무나 첫 턴으로 설정해준다
            TableManager.StartTurnOf(Random.Range(0, 4));

            ChangeGameState(State_WaitForDiscard);
        }
    }

    void WaitForDiscard(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;
        var eventCard = TableManager.GetCardByIndex(inputEvent.CardIndex);

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
                TableManager.StartTurnOfNext();

                ChangeGameState(State_WaitForDiscard);
            }
            else
            {
                WaitingNakiCard = eventCard;
                UIActivedCount = uiActived;
                Debug.Log($"UIActived. Count:{UIActivedCount}");

                ChangeGameState(State_WaitForNaki);
            }
        }
    }

    void WaitForNaki(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;

        if (!IsNakiInput(inputEvent)) { return; }
        if (WaitingNakiCard == null) { Debug.Log("이게 null이면.. 안되는데?"); }

        var formerPlayer = TableManager.GetCurrentTurnPlayer();
        formerPlayer.RemoveStashedCard(WaitingNakiCard);

        TableManager.StartTurnOf(inputEvent.PlayerIndex);

        var nextPlayer = TableManager.GetCurrentTurnPlayer();

        switch (eventType)
        {
            case "Chi":
            {
                var chiCards = new Card[]
                {
                    WaitingNakiCard,
                    TableManager.GetCardByIndex((int)inputEvent.ChiIndex.x),
                    TableManager.GetCardByIndex((int)inputEvent.ChiIndex.y)
                };

                nextPlayer.OpenCards(chiCards);

                UIActivedCount = 0;
                break;
            }

            case "Pon":
            {
                var sameOrderCards = nextPlayer.FindCardByGlobalOrder(WaitingNakiCard.GlobalOrder, 2);
                var ponCards = new Card[]
                {
                    WaitingNakiCard,
                    sameOrderCards[0],
                    sameOrderCards[1]
                };

                nextPlayer.OpenCards(ponCards);

                UIActivedCount = 0;
                break;
            }

            case "Kkan":
            {
                var sameOrderCards = nextPlayer.FindCardByGlobalOrder(WaitingNakiCard.GlobalOrder, 3);
                var kkanCards = new Card[]
                {
                    WaitingNakiCard,
                    sameOrderCards[0],
                    sameOrderCards[1],
                    sameOrderCards[2]
                };

                nextPlayer.OpenCards(kkanCards);

                UIActivedCount = 0;
                break;
            }

            case "Ron":
            {
                // 해야한다..
                break;
            }

            case "Skip":
            {
                --UIActivedCount;
                break;
            }
        }

        Debug.Log($"UIActivedCount {UIActivedCount}");
        if (UIActivedCount == 0)
        {
            TableManager.DisableUIAll();
            ChangeGameState(State_WaitForDiscard);
        }
    }

    void ChangeGameState(string state)
    {
        Debug.Log($"GameState = {state}");

        GameState = state;
    }

    bool IsNakiInput(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;
        return eventType == "Chi"
            || eventType == "Pon"
            || eventType == "Kkan"
            || eventType == "Ron"
            || eventType == "Skip";
    }
}
