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
                    var currentTable = TableManager.GetCurrentTurnPlayer();

                    if (!currentTable.Contains(eventCard))
                    {
                        // 실제 플레이에서는 현재 턴의 유저만 interact 가능하기 때문에 여기 안 옴
                        break;
                    }

                    currentTable.Discard(eventCard);

                    TableManager.AnnounceDiscard(eventCard);

                    if (!TableManager.IsAnyoneUIActived())
                    {
                        TableManager.AddNextCard();
                        TableManager.MoveToNextTable();
                    }

                    break;

                case "Chi":
                    TableManager.AddNextCard();
                    break;

                case "Pon":
                    TableManager.AddNextCard();
                    break;

                case "Kkan":
                    TableManager.AddNextCard();
                    // TODO
                    break;
            }
        }
    }
}
