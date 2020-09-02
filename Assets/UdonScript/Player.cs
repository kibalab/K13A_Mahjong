using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

public class Player : UdonSharpBehaviour
{

    private const int FULL_CARD_COUNT = 14;

    [SerializeField] public UIManager UiManager;
    [SerializeField] public GameObject CardPositions;
    [SerializeField] public KList Cards;
    [SerializeField] public KList OpenendCards;
    [SerializeField] public UIContext UIContext;
    [SerializeField] public AgariContext AgariContext;
    [SerializeField] public HandCalculator HandCalculator;
    [SerializeField] public Transform StashPositions;
    [SerializeField] public Transform nakiPoints;
    [SerializeField] public Transform nakiShapes;
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public PlayerStatus playerStatus;
    [SerializeField] public Subtitle Subtitle;
    [SerializeField] public GameObject RiichiBon;
    [SerializeField] public TableViewer TableViewer;

    [UdonSynced(UdonSyncMode.None)] public string NetworkMessage = "";
    [UdonSynced(UdonSyncMode.None)] public string PlayerName;

    public int PlayerIndex;

    private int messageNumber = 0; // 마스터 전용
    private int lastMessageNumber = -1; // 모든 유저용

    private Transform[] cardPoints;
    private Transform plusCardPosition;

    // GlobalOrder 자리에 퐁한 위치를 저장해두는 용도
    private Transform[] OpenedPonPositions;
    private int nakiCount;

    int[] stashedCards;
    int stashedCardIndex;

    public void Initialize(int playerIndex)
    {
        Cards.Clear();
        cardPoints = FindPoints();
        stashedCards = new int[34];
        stashedCardIndex = 0;
        nakiCount = 0;
        OpenedPonPositions = new Transform[34];
        PlayerIndex = playerIndex;
        playerStatus.Initialize();
        Subtitle.SetPlaytime(12.0f);
        NetworkMessage = SerializeRiichi(false);

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

    string SerializeRiichi(bool riichiMode)
    {
        var serializedString = $"{messageNumber++},Riichi,{riichiMode}";
        return serializedString;
    }
    string SerializePlayerName(string playerName)
    {
        var serializedString = $"{messageNumber++},Name,{playerName}";
        return serializedString;
    }

    public void ActiveRiichiMode()
    {
        playerStatus.IsRiichiMode = true;
        playerStatus.IsOneShotRiichi = true;
        setStashPositionRichMode();
        Debug.Log($"[Player] Request RiichiBon setActive : {true}");
        NetworkMessage = SerializeRiichi(true);
    }

    public bool IsRiichiMode()
    {
        return playerStatus.IsRiichiMode;
    }

    public void DeactiveOneShotRiichi()
    {
        playerStatus.IsOneShotRiichi = false;
    }

    public void AddCard(Card newCard, bool isFristTsumo, bool isLastTsumo, bool isByRinshan)
    {
        playerStatus.IsFirstTsumo = isFristTsumo;
        playerStatus.IsLastTsumo = isLastTsumo;
        playerStatus.IsByRinshan = isByRinshan;

        UIContext.Clear();
        AgariContext.Clear();

        var str = "Sealed : ";
        foreach (Card card in GetArray(Cards))
        {
            str += $"{card.ToString()}, ";
        }
        str += "\nOpenned : ";
        foreach (Card card in GetArray(OpenendCards))
        {
            str += $"{card.ToString()}, ";
        }
        Debug.Log(str);
        HandCalculator.CheckTenpai(GetArray(Cards), GetArray(OpenendCards), AgariContext);
        UIContext.IsTsumoable = AgariContext.IsAgariable(newCard);

        Cards.Add(newCard);

        newCard.SetOwnership(PlayerIndex);
        newCard.SetPosition(plusCardPosition.position, plusCardPosition.rotation);

        if (!UIContext.IsTsumoable && playerStatus.IsRiichiMode)
        {
            EventQueue.SetAutoDiscardEvent(newCard.YamaIndex, PlayerIndex);
            playerStatus.IsOneShotRiichi = false;
        }
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

    public void Discard(Card card)
    {
        AgariContext.Clear();
        if (Contains(card))
        {
            Cards.RemoveAt(Cards.IndexOf(card));
        }
        else
        {
            Debug.Log($"없는 카드를 제거하려고 했는데 일단 진행해봄, Card: {card.ToString()}");
        }

        stashedCards[card.GlobalOrder]++;

        var point = StashPositions.GetChild(stashedCardIndex++);
        card.SetPosition(point.position, point.rotation);
        card.SetColliderActivate(false);
        playerStatus.IsFirstOrder = false;

        SortPosition();

        HandCalculator.CheckTenpai(GetArray(Cards), GetArray(OpenendCards), AgariContext);
    }

    public void setStashPositionRichMode()
    {
        Transform[] positions;
        var CurrentLine = stashedCardIndex / 6;
        var l = 0;

        var point = StashPositions.GetChild(stashedCardIndex);
        point.rotation = Quaternion.Euler(point.rotation.eulerAngles - new Vector3(0, 90, 0));
        point.position -= point.forward * 0.0085001f;

        for (var i = stashedCardIndex + 1; i < CurrentLine + 6; i++, l++)
        {
            point = StashPositions.GetChild(i);
            point.position += point.up * 0.0085001f * 2;
        }
    }

    public void RemoveStashedCard(Card card)
    {
        if (stashedCards[card.GlobalOrder] == 0) { Debug.Log("이러면... 안되는데?"); }

        --stashedCards[card.GlobalOrder];
        --stashedCardIndex;
    }

    public void OpenCards_Pon(Card[] openTargetCards, int shapeType)
    {
        AgariContext.Clear();
        var nakiShape = GetNextNakiShape(shapeType);
        SetNakiPosition(openTargetCards, nakiShape);

        // 퐁의 경우 소명깡을 위해 nakiShape를 저장해둔다
        var ponGlobalOrder = openTargetCards[0].GlobalOrder;
        OpenedPonPositions[ponGlobalOrder] = nakiShape;

        SortPosition();
        HandCalculator.CheckTenpai(GetArray(Cards), GetArray(OpenendCards), AgariContext);
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

    public void AddKkanCount()
    {
        ++playerStatus.KkanCount;
    }

    public void OpenCards(Card[] openTargetCards, int shapeType)
    {
        AgariContext.Clear();
        playerStatus.IsMenzen = false;

        var nakiShape = GetNextNakiShape(shapeType);
        SetNakiPosition(openTargetCards, nakiShape);

        // 치, 깡의 경우 nakiShape을 저장할 필요가 없다
        Destroy(nakiShape.gameObject);

        SortPosition();
        HandCalculator.CheckTenpai(GetArray(Cards), GetArray(OpenendCards), AgariContext);
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
            pickedCard.SetOwnership(PlayerIndex);
            pickedCard.SetPosition(pointTransform.position, pointTransform.transform.rotation);
        }

        SortPosition();
    }

    public void SetColliderActive(bool active)
    {
        // 리치 중일 때는 조작이 불가능하게 함
        if (playerStatus.IsRiichiMode)
        {
            return;
        }

        foreach (Card card in Cards.Clone())
        {
            card.SetColliderActivate(active);
        }
    }

    public PlayerStatus CalculateTsumoScore()
    {
        var cards = GetArray(Cards);
        var openedCards = GetArray(OpenendCards);
        HandCalculator.RequestTsumoScore(cards, openedCards, AgariContext, playerStatus);

        return playerStatus;
    }

    public PlayerStatus CalculateRonScore()
    {
        var cards = GetArray(Cards);
        var openedCards = GetArray(OpenendCards);
        HandCalculator.RequestRonScore(cards, openedCards, AgariContext, playerStatus);

        return playerStatus;
    }

    public void SetPlayerName(string name)
    {
        PlayerName = name;
        NetworkMessage = SerializePlayerName(name);
    }

    public void _SetPlayerName(string name)
    {
        TableViewer.setPlayerName(name, PlayerIndex);
    }

    public void SetWind(string wind)
    {
        playerStatus.Wind = wind;
        TableViewer.setWInd(PlayerIndex, wind);
    }

    public void SetRoundWind(string roundWind)
    {
        playerStatus.RoundWind = roundWind;
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
    void RequestCallFunctionToAll(string funcName)
    {
        if (Networking.LocalPlayer == null)
        {
            SendCustomEvent(funcName);
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, funcName);
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

    private void Update()
    {
        if (string.IsNullOrEmpty(NetworkMessage))
        {
            return;
        }

        var splited = NetworkMessage.Split(',');
        var networkMessageNumber = int.Parse(splited[0]);

        if (lastMessageNumber != networkMessageNumber)
        {
            lastMessageNumber = networkMessageNumber;

            switch (splited[1])
            {
                case "Riichi":
                    Debug.Log($"[Player] RiichiBon setActive : {splited[2]}");
                    RiichiBon.SetActive(bool.Parse(splited[2]));
                    break;
                case "Name":
                    _SetPlayerName(splited[2]);
                    break;
            }
            

        }
    }
}