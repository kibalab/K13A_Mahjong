
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
        if (Networking.LocalPlayer == null)
        { 
            Initialize();
        }
        else 
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "_Initialize");
        }
    }

    public void _Initialize()
    {
        InitializeCards();
        cards = ShuffleCards(cards);

        InitializePlayers();
    }

    void InitializeCards()
    {
        var index = 0;

        foreach (var type in new string[3] { "만", "통", "삭" })
        {
            foreach (var number in new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                for (int i = 0; i < 4; ++i)
                {
                    var isDora = number == 5 ? (i == 3 ? true : false) : false; // 5만, 5삭, 5통만 4개중 도라 하나를 가지고있음
                    cards[index++].Initialize(type, number, isDora, EventQueue, Sprites, HandUtil);
                }
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            cards[index++].Initialize("동", 1, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("남", 2, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("서", 3, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("북", 4, false, EventQueue, Sprites, HandUtil);

            cards[index++].Initialize("백", 5, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("발", 6, false, EventQueue, Sprites, HandUtil);
            cards[index++].Initialize("중", 7, false, EventQueue, Sprites, HandUtil);
        }
    }

    void InitializePlayers()
    {
        for (int i = 0; i < players.Length; ++i)
        {
            var pickedCards = GetNextCards(13);
            var stashTable = StashTables.transform.GetChild(i);

            var player = players[i];
            player.Initialize(i, EventQueue, stashTable);
            player.SetColliderActive(false);
            player.SetCards(pickedCards);
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
