using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardManager : UdonSharpBehaviour
{
    public string positionName;
    private int cardCount = 13;
    public CardComponent[] cards;
    public GameObject[] CardPoints;
    void Start()
    {
        // 기존엔 리턴형을 만들어놓고 안 쓰고 있슴니다
        // findPoints();
        // 이럴땐 그냥 void로 하거나
        // 아니면 아래처럼 정확히 리턴해서 받는 함수를 구현하도록 하죠

        // CardPoints = FindPoints();
    }

    // 사실 start가 버그의 주범입니다
    // start 간의 순서도 바꿀 수 없고, 언제 됐는지 알기도 어려워서
    // 타이밍을 잡을 수 있으면 (보통 매니저가 있으면) 이런거 만들어서 씁니다
    public void Initialize()
    {
        CardPoints = FindPoints();
    }

    GameObject[] FindPoints()
    {
        //배열의 0~13 은 소유카드 14는 추가카드
        var cardPoints = new GameObject[14];
        for (int i = 0; i <= cardCount; i++)
        {
            Debug.Log(this.gameObject.transform.GetChild(i).name);
            cardPoints[i] = this.gameObject.transform.GetChild(i).gameObject;
        }
        return cardPoints;
    }

    public void SetCards(CardComponent[] pickedCards)
    {
        cards = pickedCards;

        for (int i = 0; i<= cardCount; i++)
        {
            var pointTransform = CardPoints[i].transform;
            cards[i].SetPosition(pointTransform.position, pointTransform.transform.rotation); 
        }

        SortCard();
    }

    public void Pickupable(bool b)
    {
        foreach (CardComponent card in cards)
        {
            card.gameObject.GetComponent<BoxCollider>().enabled = b;
        }
    }

    public CardComponent[] SortCard()
    {
        int i;
        int j;
        CardComponent temp;

        for (i = (cards.Length - 1); i >= 0; i--)
        {
            for (j = 1; j <= i; j++)
            {
                if (cards[j - 1].normalCardNumber > cards[j].normalCardNumber)
                {
                    temp = cards[j - 1];
                    cards[j - 1] = cards[j];
                    cards[j] = temp;
                }
            }
        }
        return cards;
    }
}