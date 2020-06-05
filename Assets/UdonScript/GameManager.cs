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

    void Start()
    {
        if (Networking.GetOwner(this.gameObject) == null)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

        TableManager.Initialize();

        if (Networking.LocalPlayer == null)
        {
            SettingForUnityTests();
        }

        GameState = State_WaitForStart;
    }

    void SettingForUnityTests()
    {
        TableManager.AddNextCard();
        GameState = State_WaitForDiscard;
    }
    
    void Update()
    {
        if (!EventQueue.IsQueueEmpty())
        {
            var inputEvent = EventQueue.Dequeue();

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
            TableManager.MoveTurnTo(Random.Range(0, 4));
            TableManager.AddNextCard();

            GameState = State_WaitForDiscard;
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
                TableManager.AddNextCard();
                TableManager.MoveToNextTable();

                GameState = State_WaitForDiscard;
            }
            else
            {
                UIActivedCount = uiActived;

                GameState = State_WaitForNaki;
            }
        }
    }

    void WaitForNaki(InputEvent inputEvent)
    {
        var eventType = inputEvent.EventType;
        var eventCard = TableManager.GetCardByIndex(inputEvent.CardIndex);

        if (eventType != "Chi" || eventType != "Pon" || eventType != "Kkan" || eventType != "Ron")
        {
            return;
        }

        var formerPlayer = TableManager.GetCurrentTurnPlayer();
        formerPlayer.RemoveStashedCard(eventCard);

        TableManager.MoveTurnTo(inputEvent.PlayerIndex);

        var nextPlayer = TableManager.GetCurrentTurnPlayer();

        switch (eventType)
        {
            case "Chi":
            {
                var chiCards = new Card[]
                {
                    eventCard,
                    TableManager.GetCardByIndex((int)inputEvent.ChiIndex.x),
                    TableManager.GetCardByIndex((int)inputEvent.ChiIndex.y)
                };

                nextPlayer.OpenCards(chiCards);

                UIActivedCount = 0;
                break;
            }
                

            case "Pon":
            {
                var sameOrderCards = nextPlayer.FindCardByGlobalOrder(eventCard.GlobalOrder, 2);
                var ponCards = new Card[]
                {
                    eventCard,
                    sameOrderCards[0],
                    sameOrderCards[1]
                };

                nextPlayer.OpenCards(ponCards);

                UIActivedCount = 0;
                break;
            }

            case "Kkan":
            {
                var sameOrderCards = nextPlayer.FindCardByGlobalOrder(eventCard.GlobalOrder, 3);
                var kkanCards = new Card[]
                {
                    eventCard,
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

        if (UIActivedCount == 0)
        {
            TableManager.MoveToNextTable();
            TableManager.AddNextCard();
            GameState = State_WaitForDiscard;
        }
    }
}
