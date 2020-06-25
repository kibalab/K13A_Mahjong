using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Player : UdonSharpBehaviour
{
    private const int FULL_CARD_COUNT = 14;

    [SerializeField] public int PlayerIndex;
    [SerializeField] public UIManager UiManager;
    [SerializeField] public GameObject CardPositions;
    [SerializeField] public KList Cards;
    [SerializeField] public KList OpenendCards;
    [SerializeField] public InputEvent InputEvent;
    [SerializeField] public UIContext UIContext;
    [SerializeField] public AgariContext AgariContext;
    [SerializeField] public HandCalculator HandCalculator;
    [SerializeField] public Transform StashPositions;

    private Transform[] cardPoints;
    private Transform plusCardPosition;

    int[] stashedCards;
    int stashedCardIndex;

    public void Initialize()
    {
        cardPoints = FindPoints();
        stashedCards = new int[34];
        stashedCardIndex = 0;

        UiManager.Initialize();
    }

    Transform[] FindPoints()
    {
        //배열의 0~13 은 소유카드 14는 추가카드
        var cardPoints = new Transform[FULL_CARD_COUNT];
        for (int i = 0; i < 14; i++)
        {
            var tr = CardPositions.transform.GetChild(i);

            cardPoints[i] = tr;
        }
        plusCardPosition = cardPoints[13];
        return cardPoints;
    }

    public void AddCard(Card newCard, bool isFristTsumo, bool isLastTsumo)
    {
        Cards.Add(newCard);

        newCard.SetOwnership(PlayerIndex, InputEvent);
        newCard.SetPosition(plusCardPosition.position, plusCardPosition.rotation);

        if (OpenendCards.Count() == 0)
        {
            // 실제로 리치 가능한 건 쯔모일 때만인데..
            // 여기 들어오는 과정이 깡에서도 들어와서 좀 애매하게 됐네
            // 코드 좀 다시 깎아야함
            AgariContext.Clear();

            HandCalculator.RequestRiichiable(GetArray(Cards), AgariContext);
        }
    }

    public void Discard(Card card)
    {
        var index = Cards.IndexOf(card);
        Cards.RemoveAt(index);

        stashedCards[card.GlobalOrder]++;

        var point = StashPositions.GetChild(stashedCardIndex++);
        card.SetPosition(point.position, point.rotation);
        card.SetColliderActivate(false);

        SortPosition();
    }

    public void RemoveStashedCard(Card card)
    {
        if (stashedCards[card.GlobalOrder] == 0) { Debug.Log("이러면... 안되는데?"); }

        --stashedCards[card.GlobalOrder];
        --stashedCardIndex;
    }

    public void OpenCards(Card[] cards)
    {
        foreach (var card in cards)
        {
            if (Contains(card))
            {
                Cards.RemoveAt(Cards.IndexOf(card));
                OpenendCards.Add(card);
            }

            // TODO 일단 안 보이는 곳으로 보내버리는데, 나중에 수정해야함
            card.SetPosition(new Vector3(999, 999), Quaternion.identity);
        }

        SortPosition();
    }

    public Card[] FindCardByGlobalOrder(int globalOrder, int count)
    {
        Debugging(globalOrder);

        var index = 0;
        var arr = new Card[count];

        foreach(var card in GetArray(Cards))
        {
            if (card.GlobalOrder == globalOrder)
            {
                arr[index++] = card;
            }

            if (index == count)
            {
                break;
            }
        }

        if (index != count) { Debug.Log("error on FindCardByGlobalOrder"); }
        return arr;
    }

    void Debugging(int globalOrder)
    {
        var str = "";
        foreach (var card in GetArray(Cards))
        {
            str += $"({card.GlobalOrder}) ";
        }

        str = $"Target:{globalOrder}   " + str;
        Debug.Log(str);
    }

    public void CheckNakiable(Card card, bool isDiscardedByLeftPlayer)
    {
        var now = Time.time;

        UIContext.Clear();

        HandCalculator.RequestNakiable(GetArray(Cards), UIContext, AgariContext, card, isDiscardedByLeftPlayer);
    }

    public bool IsUIActived()
    {
        return UIContext.IsAnythingActived();
    }

    public void DisableUI()
    {
        UIContext.Clear();

        UiManager.DisableButtonAll();
    }

    public bool Contains(Card card)
    {
        return Cards.Contains(card);
    }

    public void SetCards(Card[] pickedCards)
    {
        for (int i = 0; i< pickedCards.Length; ++i)
        {
            var pointTransform = cardPoints[i];
            var pickedCard = pickedCards[i];

            Cards.Add(pickedCards[i]);
            pickedCard.SetOwnership(PlayerIndex, InputEvent);
            pickedCard.SetPosition(pointTransform.position, pointTransform.transform.rotation);
        }

        SortPosition();
    }

    public void SetColliderActive(bool active)
    {
        foreach (Card card in Cards.Clone())
        {
            card.SetColliderActivate(active);
        }
    }

    void SortPosition()
    {
        Cards.Sort();
        for (var k = 0; k < Cards.Count(); ++k)
        {
            var card = (Card)Cards.At(k);
            var cardPoint = cardPoints[k];
            card.SetPosition(cardPoint.position, cardPoint.rotation);
        }
    }

    Card[] GetArray(KList list)
    {
        var objs = list.Clone();
        var cards = new Card[objs.Length];

        for (var i = 0; i < objs.Length; ++i)
        {
            // 이렇게 하나하나 바꿔주지 않으면 애러남
            // 그래서 Card 전용 KList를 만들까 생각중
            cards[i] = (Card)objs[i];
        }
        return cards;
    }

    public void NakiedCard(int[] globalOrders)
    {
        foreach(int globalOrder in globalOrders)
        {
            foreach(Card card in Cards.Clone())
            {
                if (card.GlobalOrder == globalOrder)
                {
                    OpenendCards.Add(card);
                }
            }
        }
    }
}