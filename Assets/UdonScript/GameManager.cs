using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class GameManager : UdonSharpBehaviour
{
    // 이건 씬에서 연결해놓음
    public GameObject CardPool;
    public GameObject Sprites;
    public GameObject CardTable;
    public GameObject StashTable;
    public GameObject EventQueueObject;
    public Text DebugText;

    [UdonSynced(UdonSyncMode.None)] public int turnNum = 0;

    private CardComponent[] cards;
    private CardManager[] tables;
    public CardComponent[] stashedCards;
    private EventQueue eventQueue;

    private int[] stashCount = new int[4] { 0, 0, 0, 0 };
    private string[] playerTurn = new string[4] {"東", "南", "西", "北"} ; //동>남>서>북
    private int currentCardIndex = 0;

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

        foreach (var card in cards)
        {
            DebugText.text += "[SpriteSet Start] \n";
            var spriteName = card.GetCardSpriteName();
            DebugText.text += "SpriteName : " + spriteName + "\n";
            var sprite = GetCardSprite(spriteName);
            DebugText.text += "[GetSprite]\n";
            card.SetSprite(sprite);
        }

        if (Networking.IsOwner(this.gameObject))
        {
            cards = ShuffleCards(cards);
            SetPositionCards();
        }
    }

    private void Update()
    {
        if (!eventQueue.IsQueueEmpty())
        {
            var eventCard = eventQueue.Dequeue();
            var eventType = eventCard.EventType;

            switch (eventType)
            {
                case "Discard":

                    var lastedStashedCard = eventCard;
                    if (lastedStashedCard != null)
                    {
                        stashedCards[stashedCards.Length] = lastedStashedCard;
                    }
                    tables[turnNum].AddCard(GetNextCard(), eventCard);
                    eventCard.SetColliderActivate(false);
                    var stashPoint = StashTable.transform.GetChild(turnNum).GetChild(stashCount[turnNum]++);
                    eventCard.SetPosition(stashPoint.position, stashPoint.rotation);

                    //GetNextCard().SetPosition

                    SetNextTurn();
                    break;

                case "Chi":
                case "Pon":
                case "Kang":
                    // TODO
                    break;
            }
        }
    }

    void SetNextTurn()
    {
        turnNum++;

        var nextTurnPlayer = turnNum % 4;

        for (var i = 0; i < 4; i++)
        {
            tables[i].Pickupable(i == nextTurnPlayer);
        }
    }

    void SetPositionCards()
    {
        for (int i = 0; i < tables.Length; ++i)
        {
            var pickedCards = GetNextCards(14);

            var table = tables[i];
            table.Initialize();
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
                    cards[index++].Initialize(type, number, isDora, eventQueue);
                    DebugText.text += "Card Initializing : " + index + "{Type:" + type + ", Number:" + number + ", isDora:" + isDora + "\n";
                } 
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            cards[index++].Initialize("동", 0, false, eventQueue);
            cards[index++].Initialize("남", 1, false, eventQueue);
            cards[index++].Initialize("서", 2, false, eventQueue);
            cards[index++].Initialize("북", 3, false, eventQueue);

            cards[index++].Initialize("백", 0, false, eventQueue);
            cards[index++].Initialize("발", 1, false, eventQueue);
            cards[index++].Initialize("중", 2, false, eventQueue);
        }
    }

    Sprite GetCardSprite(string spriteName)
    {
        var spriteGameObject = Sprites.transform.Find(spriteName);
        var spriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();

        return spriteRenderer.sprite;
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
