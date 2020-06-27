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
    [SerializeField] public Transform nakiPoints;
    [SerializeField] public Transform nakiShapes;

    [SerializeField] public Card testcard1;
    [SerializeField] public Card testcard2;
    [SerializeField] public Card testcard3;
    [SerializeField] public Card testcard4;

    private Transform[] cardPoints;
    private Transform plusCardPosition;

    // GlobalOrder 자리에 퐁한 위치를 저장해두는 용도
    private Transform[] OpenedPonPositions;
    private int nakiCount;

    int[] stashedCards;
    int stashedCardIndex;

    public void Initialize()
    {
        cardPoints = FindPoints();
        stashedCards = new int[34];
        stashedCardIndex = 0;
        nakiCount = 0;
        OpenedPonPositions = new Transform[34];
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
        UIContext.Clear();

        // 버리는 순간 계산된 AgariContext로 카드 받는 순간 화료 가능한지 본다
        UIContext.IsTsumoable = AgariContext.IsAgariable(newCard);

        Cards.Add(newCard);

        newCard.SetOwnership(PlayerIndex, InputEvent);
        newCard.SetPosition(plusCardPosition.position, plusCardPosition.rotation);
    }

    public void CheckRiichiable()
    {
        if (OpenendCards.Count() == 0)
        {
            AgariContext.Clear();
            HandCalculator.RequestRiichiable(GetArray(Cards), AgariContext, UIContext);
        }
    }

    public void CheckOpenOrAnkkanable(Card newCard)
    {
        var isAnkkanable = HandCalculator.IsAnKkanable(GetArray(Cards));
        var isOpenKkanable = HandCalculator.IsOpenKkanable(newCard, GetArray(OpenendCards));

        UIContext.IsKkanable = isAnkkanable || isOpenKkanable;
    }

    public void CheckOpenKkanable(Card addedCard)
    {
        UIContext.IsKkanable = HandCalculator.IsOpenKkanable(addedCard, GetArray(OpenendCards));
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

        // 버리는 순간 텐파이인지 검사해놓는다
        AgariContext.Clear();
        HandCalculator.CheckTenpai(GetArray(Cards), GetArray(OpenendCards), AgariContext);
    }

    public void RemoveStashedCard(Card card)
    {
        if (stashedCards[card.GlobalOrder] == 0) { Debug.Log("이러면... 안되는데?"); }

        --stashedCards[card.GlobalOrder];
        --stashedCardIndex;
    }

    public void OpenCards_Pon(Card[] openTargetCards, int shapeType)
    {
        var nakiShape = GetNextNakiShape(shapeType);
        SetNakiPosition(openTargetCards, nakiShape);

        // 퐁의 경우 소명깡을 위해 nakiShape를 저장해둔다
        var ponGlobalOrder = openTargetCards[0].GlobalOrder;
        OpenedPonPositions[ponGlobalOrder] = nakiShape;

        SortPosition();
    }

    public void ActiveRiichiCreateCardColliders()
    {
        // 일단 다 끈다
        SetColliderActive(false);

        var debugStr = "RiichiCreationCards = ";

        // 리치 만들어주는 것만 킨다
        foreach (var card in AgariContext.RiichiCreationCards)
        {
            debugStr += $"({card.Type}, {card.CardNumber})";

            card.SetColliderActivate(true);
            card.IsDiscardedForRiichi = true;
        }

        Debug.Log(debugStr);
    }

    public void AddOpenKkan()
    {
        var card = (Card)Cards.RemoveLast();
        var nakiShape = OpenedPonPositions[card.GlobalOrder];

        if (nakiShape == null) { Debug.Log("nakiShape가 없으면... 안되는데..?"); }

        OpenendCards.Add(card);

        var nakiCardPosition = nakiShape.GetChild(3).transform;
        card.SetPosition(nakiCardPosition.position, nakiCardPosition.rotation);

        SortPosition();
    }

    public void OpenCards(Card[] openTargetCards, int shapeType)
    {
        var nakiShape = GetNextNakiShape(shapeType);
        SetNakiPosition(openTargetCards, nakiShape);

        // 치, 깡의 경우 nakiShape을 저장할 필요가 없다
        Destroy(nakiShape.gameObject);

        SortPosition();
    }

    Transform GetNextNakiShape(int shapeType)
    {
        var nakiShape = VRCInstantiate(nakiShapes.GetChild(shapeType).gameObject);
        var nakiPoint = nakiPoints.GetChild(nakiCount++);
        nakiShape.transform.SetPositionAndRotation(nakiPoint.position, nakiPoint.rotation);

        return nakiShape.transform;
    }

    void SetNakiPosition(Card[] openTargetCards, Transform nakiShape)
    {
        for (var index = 0; index < openTargetCards.Length; index++)
        {
            var card = openTargetCards[index];

            if (Contains(card))
            {
                Cards.RemoveAt(Cards.IndexOf(card));
                OpenendCards.Add(card);
            }

            var nakiCardPosition = nakiShape.GetChild(index).transform;
            openTargetCards[index].SetPosition(nakiCardPosition.position, nakiCardPosition.rotation);
        }
    }

    public Card[] FindCardByGlobalOrder(int globalOrder, int count)
    {
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

    public int[] FindAnkkanableGlobalOrders()
    {
        return HandCalculator.GetAnkkanableAll(GetArray(Cards));
    }

    public void CheckNakiable(Card card, bool isDiscardedByLeftPlayer)
    {
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
    
    bool IsPon(Card[] cards)
    {
        if (cards.Length > 1) return false;
        var globalOrder = cards[0].GlobalOrder;

        for (var i = 1; i < cards.Length; ++i)
        {
            if (cards[i].GlobalOrder != globalOrder)
            {
                return false;
            }
        }
        return true;
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