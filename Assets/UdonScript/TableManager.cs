
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

    private Card[] yama;
    private Player[] players;
    private int currentCardIndex = 0;

    void Start()
    {
        yama = CardPool.GetComponentsInChildren<Card>();
        players = Players.GetComponentsInChildren<Player>();
    }

    public bool IsCurrentTurn(int playerIndex)
    {
        return currentTurnPlayer == playerIndex;
    }

    public Player GetCurrentTurnPlayer()
    {
        return players[currentTurnPlayer];
    }

    public void SetNextTurn()
    {
        var nextPlayerIndex = (currentTurnPlayer + 1) % 4;

        SetTurnOf(nextPlayerIndex);
    }

    public void SetTurnOf(int playerIndex)
    {
        currentTurnPlayer = playerIndex;
    }

    public Player GetPlayer(int playerIndex)
    {
        return players[playerIndex];
    }

    public void AddNextCard()
    {
        var player = GetCurrentTurnPlayer();
        var index = currentCardIndex;

        var nextCard = GetNextCard();
        var isFirstTsumo = index == 0;
        var isLastTsumo = index == yama.Length - 1;

        player.AddCard(nextCard, isFirstTsumo, isLastTsumo);

        for (var i = 0; i < 4; i++)
        {
            var active = i == currentTurnPlayer;
            players[i].SetColliderActive(active);
        }
    }

    public Card GetCardByIndex(int cardIndex)
    {
        return yama[cardIndex];
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

    public int GetUIActivedUserCount()
    {
        var count = 0;

        foreach (var player in players)
        {
            if (player.IsUIActived()) { ++count; }
        }

        return count;
    }

    public void DisableUIAll()
    {
        foreach (var player in players)
        {
            player.DisableUI();
        }
    }

    public void Initialize()
    {
        if (Networking.LocalPlayer == null)
        { 
            _Initialize();
        }
        else 
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "_Initialize");
        }
    }

    public void _Initialize()
    {
        InitializeCards();
        yama = ShuffleCards(yama);

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
                    yama[index++].Initialize(type, number, isDora, EventQueue, Sprites, HandUtil);
                }
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            yama[index++].Initialize("동", 1, false, EventQueue, Sprites, HandUtil);
            yama[index++].Initialize("남", 2, false, EventQueue, Sprites, HandUtil);
            yama[index++].Initialize("서", 3, false, EventQueue, Sprites, HandUtil);
            yama[index++].Initialize("북", 4, false, EventQueue, Sprites, HandUtil);

            yama[index++].Initialize("백", 5, false, EventQueue, Sprites, HandUtil);
            yama[index++].Initialize("발", 6, false, EventQueue, Sprites, HandUtil);
            yama[index++].Initialize("중", 7, false, EventQueue, Sprites, HandUtil);
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
        var nextCard = yama[currentCardIndex];
        nextCard.yamaIndex = currentCardIndex;

        ++currentCardIndex;

        return nextCard;
    }
}
