
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class TableManager : UdonSharpBehaviour
{
    /*LinkedInInspector*/ public GameObject CardPool;
    /*LinkedInInspector*/ public GameObject Players;
    /*LinkedInInspector*/ public CardSprites Sprites;
    /*LinkedInInspector*/ public HandCalculator HandCalculator;
    /*LinkedInInspector*/ public HandUtil HandUtil;
    /*LinkedInInspector*/ public EventQueue EventQueue;
    /*LinkedInInspector*/ public GameObject StashTables;

    [UdonSynced(UdonSyncMode.None)] public int currentTurnPlayer = 0;

    private Card[] cards;
    private Player[] players;
    private int currentCardIndex = 0;

    void Start()
    {
        cards = CardPool.GetComponentsInChildren<Card>();
        players = Players.GetComponentsInChildren<Player>();
    }

    public Player GetCurrentTurnPlayer()
    {
        return players[currentTurnPlayer];
    }

    public void MoveToNextTable()
    {
        currentTurnPlayer = (currentTurnPlayer + 1) % 4;
    }

    public void AddNextCard()
    {
        var currentTable = GetCurrentTurnPlayer();
        var nextCard = GetNextCard();
        currentTable.AddCard(nextCard);

        for (var i = 0; i < 4; i++)
        {
            var active = i == currentTurnPlayer;
            players[i].SetColliderActive(active);
        }
    }

    public Card GetCardByIndex(int cardIndex)
    {
        return cards[cardIndex];
    }

    public void AnnounceDiscard(Card card)
    {
        for (var i = 0; i < 4; i++)
        {
            if (i != currentTurnPlayer)
            {
                players[i].CheckNakiable(card);
            }
        }
    }

    public bool IsAnyoneUIActived()
    {
        foreach (var player in players)
        {
            if (player.IsUIActived()) { return true; }
        }

        return false;
    }

    public void Initialize()
    {
        // VRC�� �������� ���� �̰ɷ�
        // SendCustomNetworkEvent(NetworkEventTarget.Owner, "_Initialize"); 
        _Initialize();
    }

    public void _Initialize()
    {
        var index = 0;

        foreach (var type in new string[3] { "��", "��", "��" })
        {
            foreach (var number in new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                for (int i = 0; i < 4; ++i)
                {
                    var isDora = number == 5 ? (i == 3 ? true : false) : false; // 5��, 5��, 5�븸 4���� ���� �ϳ��� ����������
                    cards[index++].Initialize(type, number, isDora, EventQueue, Sprites, HandUtil);
                }
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            cards[index++].Initialize("��", 1, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("��", 2, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("��", 3, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("��", 4, false, EventQueue, Sprites, HandUtil);

            cards[index++].Initialize("��", 5, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("��", 6, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("��", 7, false, EventQueue, Sprites, HandUtil);
        }
    }
    public Card[] ShuffleCards(Card[] cards)
    {
        var shuffledCards = new Card[136];
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

    public void SetPositionCards()
    {
        for (int i = 0; i < players.Length; ++i)
        {
            var pickedCards = GetNextCards(13);
            var stashTable = StashTables.transform.GetChild(i);

            var table = players[i];
            table.Initialize(i, EventQueue, stashTable);
            table.SetColliderActive(false);
            table.SetCards(pickedCards);
        }
    }

    Card[] GetNextCards(int count)
    {
        var pickedCards = new Card[count];

        for (var i = 0; i < count; i++)
        {
            pickedCards[i] = GetNextCard();
        }

        return pickedCards;
    }

    Card GetNextCard()
    {
        var nextCard = cards[currentCardIndex];
        nextCard.Index = currentCardIndex;

        ++currentCardIndex;

        return nextCard;
    }
}
