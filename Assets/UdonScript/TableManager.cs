
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class TableManager : UdonSharpBehaviour
{
    [SerializeField] public GameObject CardPool;
    [SerializeField] public GameObject Players;
    [SerializeField] public CardSprites Sprites;
    [SerializeField] public HandCalculator HandCalculator;
    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public GameObject StashTables;
    [SerializeField] public Material normalMaterial;
    [SerializeField] public Material doraMaterial;

    [UdonSynced(UdonSyncMode.None)] public int currentTurnPlayer = 0;

    private Card[] yama;
    private Card[] doras;
    private Card[] rinShan;
    private Player[] players;
    private int currentCardIndex = 0;
    private int currentRinShanCardIndex = 0;

    public LogViewer LogViewer;

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
        players[currentTurnPlayer].SetColliderActive(false);
        currentTurnPlayer = playerIndex;
        players[playerIndex].SetColliderActive(true);
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

    public void AddNextRinShanCard()
    {
        var player = GetCurrentTurnPlayer();
        var index = currentCardIndex;

        var nextCard = GetNextRinShanCard();
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
        for (var playerIndex = 0; playerIndex < 4; playerIndex++)
        {
            if (playerIndex != currentTurnPlayer)
            {
                var isDiscardedByLeftPlayer = playerIndex == (currentTurnPlayer + 1) % 4;
                players[playerIndex].CheckNakiable(card, isDiscardedByLeftPlayer);
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

    public void Initialize_Master()
    {
        if (Networking.LocalPlayer != null)
        {
            LogViewer.Log($"Set Owner (TableManager, {Networking.LocalPlayer.displayName})", 0);
        }

        // 한 번만 하는 초기화
        // 1. 월드 마스터만 알면 되는 yama의 순서
        // 2. 다른 사람들을 위한 Card의 UdonSync 변수 설정
        InitializeYama_Master();
        LogViewer.Log("Yama Initalized", 0);


        // Player의 변수는 전부 Master만 알고 있으면 됨
        InitializePlayers_Master();
        LogViewer.Log("PlayersInfo Initalized", 0);
    }

    public void Initialize_All()
    {
        foreach (var card in yama)
        {
            card.syncData();
            var material = card.IsDora ? doraMaterial : normalMaterial;
            card.Initialize_All(EventQueue, HandUtil, Sprites, material);
        }

        for (int i = 0; i < players.Length; ++i)
        {
            var player = players[i];
            var stashTable = StashTables.transform.GetChild(i);
            player.Initialize_All(EventQueue, stashTable);
            LogViewer.Log($"LocalPlayer PlayerInfo Initalized (IndexID: {player.PlayerIndex})", 1);
        }
    }

    void InitializeYama_Master()
    {
        var index = 0;

        foreach (var type in new string[3] { "만", "통", "삭" })
        {
            foreach (var number in new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                for (int i = 0; i < 4; ++i)
                {
                    var isDora = number == 5 ? (i == 3 ? true : false) : false; // 5만, 5삭, 5통만 4개중 도라 하나를 가지고있음
                    yama[index++].Initialize_Master(type, number, isDora, false);
                    LogViewer.Log($"Master Card Initalized (Name: {yama[index-1].Type}{yama[index-1].CardNumber}, GlobalOrder: {yama[index-1].GlobalOrder})", 0);
                }
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            yama[index++].Initialize_Master("동", 1, false, false);
            yama[index++].Initialize_Master("남", 2, false, false);
            yama[index++].Initialize_Master("서", 3, false, false);
            yama[index++].Initialize_Master("북", 4, false, false);

            yama[index++].Initialize_Master("백", 5, false, false);
            yama[index++].Initialize_Master("발", 6, false, false);
            yama[index++].Initialize_Master("중", 7, false, false);
        }

        yama = ShuffleCards(yama);

        doras = GetNextCards(10);// 도라표시패 5패, 우라도라표시패 5패, 총 10패
        rinShan = GetNextCards(4);// 영상패(왕패) 4패
    }

    void InitializePlayers_Master()
    {
        for (int i = 0; i < players.Length; ++i)
        {
            var pickedCards = GetNextCards(13);
            var player = players[i];

            player.Initialize_Master(i);
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
        nextCard.YamaIndex = currentCardIndex;

        ++currentCardIndex;

        return nextCard;
    }

    Card GetNextRinShanCard()
    {
        var nextCard = rinShan[currentRinShanCardIndex];
        nextCard.YamaIndex = currentRinShanCardIndex;
        nextCard.IsRinShan = true;

        ++currentRinShanCardIndex;

        return nextCard;
    }
}
