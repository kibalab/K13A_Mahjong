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

    CardComponent[] cards;

    void Start()
    {
        // 프리팹에서 동적생성한 애들에 값이 제대로 대입이 안 되서
        // 그냥 136개 만들어놓고 시작하는게 속편할듯
        cards = CardPool.GetComponentsInChildren<CardComponent>();

        InitializeCards(cards);

        foreach (var card in cards)
        {
            var spriteNumber = GetCardSpriteNumber(card);
            var sprite = GetCardSprite(spriteNumber);

            card.SetSprite(sprite);
        }

        cards = ShuffleCards(cards);
    }

    CardComponent[] InitializeCards(CardComponent[] cards)
    {
        var index = 0;

        foreach (var type in new string[3] { "만", "삭", "통" })
        {
            foreach (var number in new int[9] { 1, 2, 3, 4, 5, 6, 7, 8, 9 })
            {
                for (int i = 0; i < 4; ++i)
                {
                    var isDora = (i == 3 ? true : false);
                    cards[index++].Initialize(type, number, isDora);
                }
            }
        }
        for (int i = 0; i < 4; ++i)
        {
            cards[index++].Initialize("백", 0, false);
            cards[index++].Initialize("중", 0, false);
            cards[index++].Initialize("발", 0, false);
            cards[index++].Initialize("동", 0, false);
            cards[index++].Initialize("서", 0, false);
            cards[index++].Initialize("남", 0, false);
            cards[index++].Initialize("북", 0, false);
        }

        UnityEngine.Debug.Log("total index = " + index);

        return cards;
    }

    Sprite GetCardSprite(int spriteNumber)
    {
        UnityEngine.Debug.Log("spriteNumber = " + spriteNumber);
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

        return UnityEngine.Random.Range(1, 4) * 10 + UnityEngine.Random.Range(1, 10);

        // 이거 밑에꺼 잘 작동안함.. 테스트용으로 위처럼 씀
        if (type.Equals("백") || type.Equals("중") || type.Equals("발"))
        {
            s = 42 + cardNum;
        }
        else if (type.Equals("동") || type.Equals("서") || type.Equals("남") || type.Equals("북"))
        {
            s = 38 + cardNum;
        }
        else if (type.Equals("만"))
        {
            s = 11 + cardNum;
        }
        else if (type.Equals("삭"))
        {
            s = 29 + cardNum;
        }
        else if (type.Equals("통"))
        {
            s = 20 + cardNum;
        }
        return s;
    }
}
