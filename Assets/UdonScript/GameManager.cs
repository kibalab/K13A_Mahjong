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
    [UdonSynced(UdonSyncMode.None)] public int UIInputAwaiting = 0;

    void Start()
    {
        if (Networking.GetOwner(this.gameObject) == null)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }

        TableManager.Initialize();
        TableManager.AddNextCard();
    }

    private void Update()
    {
        switch (GameState)
        {
            case "WaitForStart":
            case "InitializeGame":
            case "AfterTsumo":
            case "WaitForDiscard":
            case "WaitForNaki":
            case "Calculating":
            case "MoveToNextRound":
            case "EndOfGame":
            default:
                // 일단은 STATE 구분 안 하고 몰아넣음
                DefaultActionForTests();
                break;
        }
    }

    void DefaultActionForTests()
    {
        if (!EventQueue.IsQueueEmpty())
        {
            var inputEvent = EventQueue.Dequeue();
            var eventType = inputEvent.EventType;
            var eventCard = TableManager.GetCardByIndex(inputEvent.CardIndex);

            inputEvent.Clear();

            switch (eventType)
            {
                case "Discard":
                    {
                        var currentTable = TableManager.GetCurrentTurnPlayer();
                        currentTable.Discard(eventCard);

                        TableManager.AnnounceDiscard(eventCard);

                        var uiActived = TableManager.GetUIActivedUserCount();
                        if (uiActived == 0)
                        {
                            TableManager.AddNextCard();
                            TableManager.MoveToNextTable();
                        }
                        else
                        {
                            UIInputAwaiting = uiActived;
                        }

                        break;
                    }

                case "Chi":
                    {
                        var formerPlayer = TableManager.GetCurrentTurnPlayer();
                        formerPlayer.RemoveStashedCard(eventCard);

                        TableManager.MoveTurnTo(inputEvent.PlayerIndex);

                        var nextPlayer = TableManager.GetCurrentTurnPlayer();
                        var chiCards = new Card[]
                        {
                            eventCard,
                            TableManager.GetCardByIndex((int)inputEvent.ChiIndex.x),
                            TableManager.GetCardByIndex((int)inputEvent.ChiIndex.y)
                        };

                        nextPlayer.OpenCards(chiCards);

                        UIInputAwaiting = 0;
                        break;
                    }


                case "Pon":
                    {
                        var formerPlayer = TableManager.GetCurrentTurnPlayer();
                        formerPlayer.RemoveStashedCard(eventCard);

                        TableManager.MoveTurnTo(inputEvent.PlayerIndex);

                        var nextPlayer = TableManager.GetCurrentTurnPlayer();
                        var sameOrderCards = nextPlayer.FindCardByGlobalOrder(eventCard.GlobalOrder, 2);
                        var ponCards = new Card[]
                        {
                            eventCard,
                            sameOrderCards[0],
                            sameOrderCards[1]
                        };

                        nextPlayer.OpenCards(ponCards);

                        UIInputAwaiting = 0;
                        break;
                    }

                case "Kkan":
                    {
                        var formerPlayer = TableManager.GetCurrentTurnPlayer();
                        formerPlayer.RemoveStashedCard(eventCard);

                        TableManager.MoveTurnTo(inputEvent.PlayerIndex);

                        var nextPlayer = TableManager.GetCurrentTurnPlayer();
                        var sameOrderCards = nextPlayer.FindCardByGlobalOrder(eventCard.GlobalOrder, 3);
                        var ponCards = new Card[]
                        {
                            eventCard,
                            sameOrderCards[0],
                            sameOrderCards[1],
                            sameOrderCards[2]
                        };

                        nextPlayer.OpenCards(ponCards);

                        UIInputAwaiting = 0;
                        break;
                    }

                case "Skip":
                    --UIInputAwaiting;

                    if (UIInputAwaiting == 0)
                    {
                        TableManager.AddNextCard();
                        TableManager.MoveToNextTable();
                    }

                    break;
            }
        }
    }
}
