
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

public class TableManager : UdonSharpBehaviour
{
    [SerializeField] public GameObject CardPool;
    [SerializeField] public GameObject Players;
    [SerializeField] public CardSprites Sprites;
    [SerializeField] public HandCalculator HandCalculator;
    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public Material normalMaterial;
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public GameObject DoraViewer;
    [SerializeField] public TableViewer TableViewer;


    public int currentTurnPlayer = 0;
    [UdonSynced(UdonSyncMode.None)] public string NetworkMessage = "";

    public string lastedDoraSpriteName = "";

    public bool randomCard = true;

    public int currentDorasCardIndex = 0;

    private int messageNumber = 0; // 마스터 전용
    private int lastMessageNumber = -1; // 모든 유저용
    private Card[] yama;
    private Card[] doras;
    private Card[] rinShan;
    private Player[] players;
    private int currentCardIndex = 0;
    private int currentRinShanCardIndex = 0;
    

    private string currentRoundWind;

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
        LogViewer.Log($"Turn Changed {currentTurnPlayer} -> {playerIndex}", 1);

        players[currentTurnPlayer].SetColliderActive(false, false);
        currentTurnPlayer = playerIndex;
        players[playerIndex].SetColliderActive(true, false);
    }

    public void SetRoundWind(string roundWind)
    {
        currentRoundWind = roundWind;
        // 계산 편의 상 현재 방위를 플레이어에게 알려준다
        foreach (var player in players)
        {
            player.SetRoundWind(roundWind);
        }
    }

    public void SetNextRoundWind(string CurrentRoundWind)
    {
        string[] winds = new string[] { "East", "South", "West", "North", "East" };
        for (var i = 0; i < 5; i++)
        {
            if (winds[i] == CurrentRoundWind)
            {
                SetRoundWind(winds[(i + 1) % 4]);
            }
        }
    }

    public Player GetPlayer(int playerIndex)
    {
        return players[playerIndex];
    }

    public void AddNextCard()
    {
        LogViewer.Log($"AddNextCard Player:{currentTurnPlayer} YamaIndex:{currentCardIndex}", 1);

        var player = GetCurrentTurnPlayer();
        var index = currentCardIndex;

        var nextCard = GetNextCard();
        if (nextCard != null)
        {
            var isFirstTsumo = index == 0;
            var isLastTsumo = index == yama.Length - 1;

            player.AddCard(nextCard, isFirstTsumo, isLastTsumo, false);
            player.CheckOpenOrAnkkanable(nextCard); // 소명깡 or 안깡
            player.CheckRiichiable(); // 쯔모에서만 리치를 봄 

            ActiveCurrentPlayerColliders();
        }
    }

    public void AddNextRinShanCard()
    {
        var player = GetCurrentTurnPlayer();
        var nextCard = GetNextRinShanCard();
        var dora = GetNextDoraCard();

        player.AddCard(nextCard, false, false, true);
        player.CheckOpenOrAnkkanable(nextCard); // 소명깡 or 안깡
        player.CheckRiichiable(); // 쯔모에서만 리치를 봄 

        if (nextCard != null && dora != null)
        {
            var doraGlobalIndex = -1;
            if(dora.GlobalOrder == HandUtil.GetManEndGlobalOrder())
            {
                doraGlobalIndex = HandUtil.GetManStartGlobalOrder();
            }else if (dora.GlobalOrder == HandUtil.GetPinEndGlobalOrder())
            {
                doraGlobalIndex = HandUtil.GetPinStartGlobalOrder();
            }else if (dora.GlobalOrder == HandUtil.GetSouEndGlobalOrder())
            {
                doraGlobalIndex = HandUtil.GetSouStartGlobalOrder();
            }else if (dora.GlobalOrder == HandUtil.GetEastCardNumber())
            {
                doraGlobalIndex = HandUtil.GetNorthCardNumber();
            }else if (dora.GlobalOrder == HandUtil.GetWhiteCardNumber())
            {
                doraGlobalIndex = HandUtil.GetRedCardNumber();
            }
            else
            {
                doraGlobalIndex = dora.GlobalOrder + 1;
            }

            SetDoraMaterials(doraGlobalIndex);
            setDoraViewerNextCard(dora.GetCardSpriteName());

            ActiveCurrentPlayerColliders();
        }
    }

    public void SetDoraMaterials(int globalOrder)
    {
        foreach (Card card in yama)
        {
            card.SetAsDora(card.GlobalOrder == globalOrder);
        }
    }

    void ActiveCurrentPlayerColliders()
    {
        for (var i = 0; i < 4; i++)
        {
            var active = i == currentTurnPlayer;
            players[i].SetColliderActive(active, false);
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
                var player = players[playerIndex];

                var isDiscardedByLeftPlayer = playerIndex == (currentTurnPlayer + 1) % 4;
                player.CheckNakiable(card, isDiscardedByLeftPlayer);
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

    public void DisableOneShotRiichiAll()
    {
        foreach (var player in players)
        {
            player.DisableUI();
        }
    }

    public void ResetTable()
    {
        //다음라운드로 전환을 위한 테이블 전체 초기화
        TableViewer.hideDisplay();
        currentTurnPlayer = 0;
        currentCardIndex = 0;
        currentRinShanCardIndex = 0;
        currentDorasCardIndex = 0;

        resetDoraViewer();

        Initialize();

        refreshSprites();

        foreach (Player p in players)
        {
            p.AgariContext.Clear();
            p.playerStatus.Initialize();
        }

        SetTurnOf(0);

        AddNextCard();
    }

    public void refreshSprites()
    {
        foreach (Card card in yama)
        {
            card.refreshSprite();
        }
    }

    public void Initialize()
    {
        InitializeYama();
        LogViewer.Log("Yama Initalized", 0);

        var firstDora =  GetNextDoraCard();


        var doraGlobalIndex = -1;
        if (firstDora.GlobalOrder == HandUtil.GetManEndGlobalOrder())
        {
            doraGlobalIndex = HandUtil.GetManStartGlobalOrder();
        }
        else if (firstDora.GlobalOrder == HandUtil.GetPinEndGlobalOrder())
        {
            doraGlobalIndex = HandUtil.GetPinStartGlobalOrder();
        }
        else if (firstDora.GlobalOrder == HandUtil.GetSouEndGlobalOrder())
        {
            doraGlobalIndex = HandUtil.GetSouStartGlobalOrder();
        }
        else if (firstDora.GlobalOrder == HandUtil.GetEastCardNumber())
        {
            doraGlobalIndex = HandUtil.GetNorthCardNumber();
        }
        else if (firstDora.GlobalOrder == HandUtil.GetWhiteCardNumber())
        {
            doraGlobalIndex = HandUtil.GetRedCardNumber();
        }
        else
        {
            doraGlobalIndex = firstDora.GlobalOrder + 1;
        }

        SetDoraMaterials(doraGlobalIndex);
        setDoraViewerNextCard(firstDora.GetCardSpriteName());
        LogViewer.Log($"Set First Dora : {firstDora}", 0);

        for (int i = 0; i < players.Length; ++i)
        {
            var pickedCards = GetNextCards(13);
            var player = players[i];

            player.Initialize(i);
            player.SetCards(pickedCards);
        }

        LogViewer.Log("PlayersInfo Initalized", 0);

        
    }

    public void InitializeYama()
    {
        var index = 0;

        foreach (var type in new string[3] { "만", "통", "삭" })
        {
            foreach (var number in new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                for (int i = 0; i < 4; ++i)
                {
                    yama[index].resetCard();
                    var isDora = number == 5 ? (i == 3 ? true : false) : false; // 5만, 5삭, 5통만 4개중 도라 하나를 가지고있음
                    yama[index++].Initialize_Master(type, number, isDora);
                }
            }
        }

        for (int i = 0; i < 4; ++i)
        {
            yama[index++].Initialize_Master("동", 1, false);
            yama[index++].Initialize_Master("남", 2, false);
            yama[index++].Initialize_Master("서", 3, false);
            yama[index++].Initialize_Master("북", 4, false);

            yama[index++].Initialize_Master("백", 5, false);
            yama[index++].Initialize_Master("발", 6, false);
            yama[index++].Initialize_Master("중", 7, false);
        }

        if (randomCard)
        {
            yama = ShuffleCards(yama);
        } 

        for (var i = 0; i < yama.Length; ++i)
        {
            yama[i].YamaIndex = i;
        }

        doras = GetNextCards(10);// 도라표시패 5패, 우라도라표시패 5패, 총 10패
        rinShan = GetNextCards(4);// 영상패(왕패) 4패
    }

    public Card[] ShuffleCards(Card[] cards)
    {
        var shuffledCards = new Card[136];
        var yetShuffledCount = 136 - 1;
        var shuffledIndex = 0;

        while (yetShuffledCount >= 0)
        {
            
            var picked = Random.Range(0, yetShuffledCount + 1);
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
        if (currentCardIndex == yama.Length)
        {
            EventQueue.AnnounceDraw("ByYamaExhausted");
            return null;
        }

        var nextCard = yama[currentCardIndex];

        ++currentCardIndex;

        return nextCard;
    }

    Card GetNextRinShanCard()
    {
        if (currentRinShanCardIndex == rinShan.Length)
        {
            EventQueue.AnnounceDraw("ByFourKkan");
            return null;
        }

        var nextCard = rinShan[currentRinShanCardIndex];
        nextCard.IsRinShan = true;

        ++currentRinShanCardIndex;

        return nextCard;
    }
    
    string SerializeDora(string doraName)
    {
        var serializedString = $"{messageNumber++},{doraName}";
        return serializedString;
    }

    Card GetNextDoraCard()
    {
        Debug.Log($"currentDorasCardIndex : {currentDorasCardIndex}, doras.Length : {doras.Length}");
        if (currentDorasCardIndex == doras.Length)
        {
            EventQueue.AnnounceDraw("ByFourKkan");
            return null;
        }

        var nextCard = doras[currentDorasCardIndex];

        Debug.Log($"nextCard : {nextCard.ToString()}");

        lastedDoraSpriteName = nextCard.GetCardSpriteName();

        NetworkMessage = SerializeDora(lastedDoraSpriteName);



        return nextCard;
    }

    private void Update()
    {
        var player = GetCurrentTurnPlayer();

        if (GetPlayer(0).UIContext.IsAnythingActived() || GetPlayer(1).UIContext.IsAnythingActived() || GetPlayer(2).UIContext.IsAnythingActived() || GetPlayer(3).UIContext.IsAnythingActived())
        {
            player.SetColliderActive(false, false);
        }
        else
        {
            player.SetColliderActive(true, false);
        }

        if (string.IsNullOrEmpty(NetworkMessage))
        {
            return;
        }

        var splited = NetworkMessage.Split(',');
        var networkMessageNumber = int.Parse(splited[0]);

        if (lastMessageNumber != networkMessageNumber)
        {
            lastMessageNumber = networkMessageNumber;
            lastedDoraSpriteName = splited[1];

            setDoraViewerNextCard(lastedDoraSpriteName);
            ++currentDorasCardIndex;
        }
    }

    void setDoraViewerNextCard(string spriteName)
    {
        var doraDisplay = DoraViewer.transform.GetChild(currentDorasCardIndex);
        doraDisplay.GetComponent<Image>().color = Color.white;
        doraDisplay.GetChild(0).GetComponent<Image>().sprite = Sprites.FindSprite(spriteName);
    }

    void resetDoraViewer()
    {
        for(var i = 0; i< DoraViewer.transform.childCount; i++)
        {
            var doraDisplay = DoraViewer.transform.GetChild(i);
            doraDisplay.GetComponent<Image>().color = new Color32(128, 128, 128, 128);
            doraDisplay.GetChild(0).GetComponent<Image>().sprite = Sprites.FindSprite("None");
        }
    }

    public bool IsReady()
    {
        if (!Networking.IsObjectReady(gameObject)) { return false; }

        foreach (var card in yama)
        {
            if (!Networking.IsObjectReady(card.gameObject))
            {
                return false;
            }
        }

        foreach (var player in players)
        {
            if (!Networking.IsObjectReady(player.gameObject))
            {
                return false;
            }
        }

        return true;
    }

    public void SetSubtitleAllPlayers(string name, string text)
    {
        foreach (var player in players)
        {
            player.Subtitle.SetSubtitle(name, text);
        }
    }
}
