using UdonSharp;
using UnityEngine;
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
    
    public CardComponent[] cards;
    public CardManager[] tables;
    public EventQueue eventQueue;


    public int turnNum = 0;
    public string[] playerTurn = new string[4] {"東", "南", "西", "北"} ; //동>남>서>북

    private int currentCardIndex = 0;

    void Start()
    {
        
        // 프리팹에서 동적생성한 애들에 값이 제대로 대입이 안 되서
        // 그냥 136개 만들어놓고 시작하는게 속편할듯
        cards = CardPool.GetComponentsInChildren<CardComponent>();
        tables = CardTable.GetComponentsInChildren<CardManager>();
        eventQueue = EventQueueObject.GetComponentInChildren<EventQueue>();

        // 밑에처럼 생성 하자마자 갖다쓰면 조용하게 안됨 (Initialize가 안불림)
        // 생성되고 "조금 있다가" 갖다쓰면 Initialize가 불림
        // 우동비헤비어의 초기화 시간이 필요한 듯 한데 정말 짜증이 난다
        /*
        var prefab = cards[0].gameObject;
        var newCard = VRCInstantiate(cards[0].gameObject);
        var cardComponent = gg.GetComponentInChildren<CardComponent>();
        cardComponent.Initialize("만", 5, true);
        */

        InitializeCards(cards);

        foreach (var card in cards)
        {
            var spriteNumber = card.NormalCardNumber = GetCardSpriteNumber(card);
            var sprite = GetCardSprite(spriteNumber);

            card.SetSprite(sprite);
        }

        cards = ShuffleCards(cards);
        SetPositionCards();
    }
    
    //public CardComponent[] StashedCards = new CardComponent[70]; // 이부분 U#에서 에러남
    private void Update()
    {
        if (!eventQueue.IsQueueEmpty())
        {
            CardComponent lastedStashedCard = eventQueue.Dequeue();
            if (lastedStashedCard != null)
            {
                //StashedCards[StashedCards.Length] = lastedStashedCard;
            }

            turnNum++;
            if (turnNum >= 4) turnNum = 0; 
        }
    }

    void SetPositionCards()
    {
        // 아래 코드는 책임이 너무 많습니다
        // 카드를 고르고 테이블의 각 칸에 할당하는 책임까지 갖고 있네요

        //for(int i = 1; i <= tables.Length; ++i)
        //{
        //    tables[i - 1].cards = new CardComponent[14];
        //    for (int j = 1; j <= 14; ++j)
        //    {
        //        tables[i - 1].cards[j - 1] = cards[i * j];
        //        //Transform cardPoint = tables[i - 1].transform.GetChild(j-1).transform; // 동:0 남:1 서:2 북:3  배패 개수:14개
        //        //Debug.Log(cardPoint.gameObject.name);
        //        //cards[i * j].SetPosition(cardPoint.position, cardPoint.rotation);

        //    }
        //    tables[i - 1].setCards();
        //}

        // 1. 14개의 카드를 고른다
        // 2. 카드 테이블에 넘겨준다
        // 3. 나머지는 카드 테이블이 알아서 하게 합시다

        for (int i = 0; i < tables.Length; ++i)
        {
            var pickedCards = GetNextCards(14);

            var table = tables[i];
            table.Initialize();
            table.SetCards(pickedCards);
        }
    }

    // 이렇게 GetNextCard를 구현한 다음
    // 그걸 활용해서 GetNextCards를 짜는게 종종 있어요 기억해두세요

    CardComponent[] GetNextCards(int count)
    {
        var pickedCards = new CardComponent[count];

        for (var i = 0; i < 14; i++)
        {
            pickedCards[i] = GetNextCard();
        }

        return pickedCards;
    }

    CardComponent GetNextCard()
    {
        //UnityEngine.Debug.Log("Get Card at " + currentCardIndex);
        return cards[currentCardIndex++];
    }

    void InitializeCards(CardComponent[] cards)
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

        //UnityEngine.Debug.Log("total index = " + index);
    }

    Sprite GetCardSprite(int spriteNumber)
    {
        //UnityEngine.Debug.Log("spriteNumber = " + spriteNumber);
        var spriteGameObject = Sprites.transform.Find(spriteNumber.ToString());
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

    private int GetCardSpriteNumber(CardComponent cardComponent)
    {
        int s = -1;
        var type = cardComponent.Type;
        var cardNum = cardComponent.CardNumber;
        bool isDora = cardComponent.IsDora;

        //return UnityEngine.Random.Range(1, 4) * 10 + UnityEngine.Random.Range(1, 10); // 아래문제를 해결 했음으로 주석처리함

        // 이거 밑에꺼 잘 작동안함.. 테스트용으로 위처럼 씀
        // ㄴ 이거 CardNumber가 0부터 시작한다는 생각으로 지정함 (현재는 1부터이기에 값을 1씩 내렸음)
        //    그리고 도라 Sprite때문에 한칸씩 밀리길래 도라표시를 위해 예외를 추가함
        if (type.Equals("백") || type.Equals("발") || type.Equals("중")) // 백발중, 동남서북 스프라이트 표시 정상적으로 안됨
        {
            s = 45 + cardNum;
        }
        else if (type.Equals("동") || type.Equals("남") || type.Equals("서") || type.Equals("북")) //마작 방위패 순서 : 동남서북
        {
            s = 41 + cardNum;
        }
        else if (type.Equals("만"))
        {
            s = 10 + cardNum;
            if (cardNum >= 5) s++;
            if(isDora) s++;
        }
        else if (type.Equals("삭"))
        {
            s = 30 + cardNum;
            if (cardNum >= 6) s++;
            if (isDora) s++;
        }
        else if (type.Equals("통"))
        {
            s = 20 + cardNum;
            if (cardNum >= 7) s++;
            if (isDora) s++;
        }
        return s;
    }
}
