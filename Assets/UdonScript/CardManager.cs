using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardManager : UdonSharpBehaviour
{
    private const int CARD_COUNT = 13;

    public string positionName;
    public CardComponent[] cards;
    public GameObject[] CardPoints;

    public void Initialize()
    {
        CardPoints = FindPoints();
    }

    GameObject[] FindPoints()
    {
        //배열의 0~13 은 소유카드 14는 추가카드
        var cardPoints = new GameObject[14];
        for (int i = 0; i <= CARD_COUNT; i++)
        {
            //Debug.Log(this.gameObject.transform.GetChild(i).name);
            cardPoints[i] = this.gameObject.transform.GetChild(i).gameObject;
        }
        return cardPoints;
    }

    public void AddCard(CardComponent plusCard, CardComponent stashCard) //쯔모 추가패
    {
        /*CardComponent tmp = cards[cardCount];
        plusCard.SetPosition(tmp.transform.position, tmp.transform.rotation);
        cards[cardCount] = plusCard;
        for (var i = 0; i<14; i++)
        {
            if(cards[i] == stashCard)
            {
                
                cards[i] = tmp;
                tmp.SetPosition(CardPoints[i].transform.position, CardPoints[i].transform.rotation);
            }
        }*/
        var i = 0;
        for (; i < 14; i++)
        {
            if (cards[i] == stashCard)
            {
                break;
            }
        }
        plusCard.SetPosition(cards[13].transform.position, cards[13].transform.rotation);
        cards[13].SetPosition(cards[i].transform.position, cards[i].transform.rotation);
        cards[i] = cards[13];
        cards[13] = plusCard;
        SortCard();
    }

    public bool Contains(CardComponent card)
    {
        foreach (var myCard in cards)
        {
            if (card == myCard)
            {
                return true;
            }
        }
        return false;
    }

    public void SetCards(CardComponent[] pickedCards)
    {
        cards = pickedCards;

        for (int i = 0; i<= CARD_COUNT; i++)
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
        Vector3 tTump;

        for (i = (cards.Length - 2); i >= 0; i--)
        {
            for (j = 1; j <= i; j++)
            {
                if (cards[j - 1].NormalCardNumber > cards[j].NormalCardNumber)
                {
                    tTump = cards[j - 1].transform.position;
                    cards[j - 1].transform.position = cards[j].transform.position;
                    cards[j].transform.position = tTump;

                    temp = cards[j - 1];
                    cards[j - 1] = cards[j];
                    cards[j] = temp;
                }
            }
        }
        return cards;
    }
}