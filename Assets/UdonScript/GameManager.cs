using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class GameManager : UdonSharpBehaviour
{
    // 이건 씬에서 연결해놓음
    public GameObject CardPool;
    public CardSprites Sprites;
    public GameObject CardTable;
    public GameObject StashTable;
    public GameObject EventQueueObject;
    public HandCalculator HandCalculator;
    public Text DebugText;

    [UdonSynced(UdonSyncMode.None)] public int turnNum = 0;

    private CardComponent[] cards;
    private CardManager[] tables;
    public CardComponent[] stashedCards;
    private EventQueue eventQueue;
    private Naki[] nakiManagers;

    private int[] stashCount = new int[4] { 0, 0, 0, 0 };
    private string[] playerTurn = new string[4] {"東", "北", "西", "南" } ; //동>북>서>남
    private int currentCardIndex = 0;
    private int currentStashIndex = 0;


    void Start()
    {
        DebugText.text = "";

        cards = CardPool.GetComponentsInChildren<CardComponent>();
        tables = CardTable.GetComponentsInChildren<CardManager>();
        eventQueue = EventQueueObject.GetComponentInChildren<EventQueue>();
        stashedCards = new CardComponent[70];

        if (Networking.GetOwner(this.gameObject) == null)
        {
            Networking.SetOwner(Networking.LocalPlayer, this.gameObject);
        }
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "InitializeCards");
        InitializeCards();
        /*
        if (Networking.IsOwner(this.gameObject))
        {
            DebugText.text = "Owner True";
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "InitializeCards"); // VRC에 릴리즈 할때는 이걸로
        }
        else
        {
            DebugText.text = "Owner False";
            InitializeCards(); // 유니티에서 테스트할떈 이걸로
        }
        */

        //InitializeCards();
        nakiManagers = this.gameObject.GetComponentsInChildren<Naki>();
        
        foreach (Naki nakiManager in nakiManagers)
        {
            nakiManager.Initialized(eventQueue);
        }

        if (Networking.IsOwner(this.gameObject))
        {
            cards = ShuffleCards(cards);
            SetPositionCards();
        }

        SetNextCard();
    }

    private void Update()
    {
        if (!eventQueue.IsQueueEmpty())
        {
            var eventCard = eventQueue.Dequeue();
            var eventType = eventCard.EventType;
            DebugText.text += "CardName : " + eventCard.Type + ", " + eventCard.CardNumber + "\n";
            switch (eventType)
            {
                case "Discard":

                    var currentTurnPlayer = GetCurrentTurnPlayer();
                    var currentTable = tables[currentTurnPlayer];

                    if (!currentTable.Contains(eventCard))
                    {
                        // 실제 플레이에서는 현재 턴의 유저만 interact 가능하기 때문에 여기 안 옴
                        break;
                    }

                    var lastedStashedCard = eventCard;
                    if (lastedStashedCard != null)
                    {
                        stashedCards[currentStashIndex++] = lastedStashedCard;
                    }

                    currentTable.Discard(eventCard);
                    eventCard.SetColliderActivate(false);
                    var stashPoint = StashTable.transform.GetChild(currentTurnPlayer).GetChild(stashCount[currentTurnPlayer]++);
                    eventCard.SetPosition(stashPoint.position, stashPoint.rotation);

                    for (var i =0; i<4; i++)
                    {
                        if (i != currentTurnPlayer)
                        {
                            Debug.Log("FindShunzzTable : " + playerTurn[i]); 
                            Debug.Log("Stashed Card : " + eventCard.CardNumber + eventCard.Type);
                            nakiManagers[i].findShunzz_Test(tables[i].cards, eventCard);
                        }
                    }

                    turnNum++;


                    SetNextCard();
                    break;

                case "Chi":
                case "Pon":
                case "Kkan":
                    // TODO
                    break;
            }
        }
    }

    void SetNextCard()
    {
        var currentTurnPlayer = GetCurrentTurnPlayer();
        var currentTable = tables[currentTurnPlayer];
        currentTable.AddCard(GetNextCard());

        for (var i = 0; i < 4; i++)
        {
            tables[i].Pickupable(i == currentTurnPlayer);
        }
    }

    int GetCurrentTurnPlayer()
    {
        return turnNum % 4;
    }

    void SetPositionCards()
    {
        for (int i = 0; i < tables.Length; ++i)
        {
            var pickedCards = GetNextCards(13);

            var table = tables[i];
            table.Initialize(HandCalculator);
            table.Pickupable(false);
            table.SetCards(pickedCards);
        }
    }

    CardComponent[] GetNextCards(int count)
    {
        var pickedCards = new CardComponent[count];

        for (var i = 0; i < count; i++)
        {
            pickedCards[i] = GetNextCard();
        }

        return pickedCards;
    }

    CardComponent GetNextCard()
    {
        return cards[currentCardIndex++];
    }

    //SendCustomEvent로 이벤트 호출은 public 함수밖에 안됨
    public void InitializeCards()
    {
        var index = 0;

        foreach (var type in new string[3] { "만", "삭", "통" })
        {
            foreach (var number in new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                for (int i = 0; i < 4; ++i)
                {
                    var isDora = number == 5 ? (i == 3 ? true : false) : false; // 5만, 5삭, 5통만 4개중 도라 하나를 가지고있음
                    cards[index++].Initialize(type, number, isDora, eventQueue, Sprites);
                    DebugText.text += "Card Initializing : " + index + "{Type:" + type + ", Number:" + number + ", isDora:" + isDora + "\n";
                } 
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            cards[index++].Initialize("동", 0, false, eventQueue, Sprites);
            cards[index++].Initialize("남", 1, false, eventQueue, Sprites);
            cards[index++].Initialize("서", 2, false, eventQueue, Sprites);
            cards[index++].Initialize("북", 3, false, eventQueue, Sprites);

            cards[index++].Initialize("백", 0, false, eventQueue, Sprites);
            cards[index++].Initialize("발", 1, false, eventQueue, Sprites);
            cards[index++].Initialize("중", 2, false, eventQueue, Sprites);
        }
    }

    public CardComponent[] ShuffleCards(CardComponent[] cards)
    {
        var shuffledCards = new CardComponent[136];
        var yetShuffledCount = 136 - 1;
        var shuffledIndex = 0;

        while (yetShuffledCount >= 0)
        {
            var picked = UnityEngine.Random.Range(0, yetShuffledCount + 1);
            shuffledCards[shuffledIndex] = cards[picked];
            cards[picked] = cards[yetShuffledCount];

            yetShuffledCount--;
            shuffledIndex++;
        }
        return shuffledCards;
    }
}
