using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class GameManager : UdonSharpBehaviour
{
    // �̰� ������ �����س���
    public GameObject CardPool;
    public GameObject Sprites;
    public GameObject CardTable;

    CardComponent[] cards;

    void Start()
    {
        // �����տ��� ���������� �ֵ鿡 ���� ����� ������ �� �Ǽ�
        // �׳� 136�� �������� �����ϴ°� �����ҵ�
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

        foreach (var type in new string[3] { "��", "��", "��" })
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
            cards[index++].Initialize("��", 0, false);
            cards[index++].Initialize("��", 0, false);
            cards[index++].Initialize("��", 0, false);
            cards[index++].Initialize("��", 0, false);
            cards[index++].Initialize("��", 0, false);
            cards[index++].Initialize("��", 0, false);
            cards[index++].Initialize("��", 0, false);
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

        // �̰� �ؿ��� �� �۵�����.. �׽�Ʈ������ ��ó�� ��
        if (type.Equals("��") || type.Equals("��") || type.Equals("��"))
        {
            s = 42 + cardNum;
        }
        else if (type.Equals("��") || type.Equals("��") || type.Equals("��") || type.Equals("��"))
        {
            s = 38 + cardNum;
        }
        else if (type.Equals("��"))
        {
            s = 11 + cardNum;
        }
        else if (type.Equals("��"))
        {
            s = 29 + cardNum;
        }
        else if (type.Equals("��"))
        {
            s = 20 + cardNum;
        }
        return s;
    }
}
