using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardManager : UdonSharpBehaviour
{
    private const int FULL_CARD_COUNT = 14;

    public string positionName;
    public CardComponent[] cards;
    public GameObject[] CardPoints;

    Transform plusCardPosition;

    public void Initialize()
    {
        CardPoints = FindPoints();
        cards = new CardComponent[FULL_CARD_COUNT];
    }

    GameObject[] FindPoints()
    {
        //배열의 0~13 은 소유카드 14는 추가카드
        var cardPoints = new GameObject[FULL_CARD_COUNT];
        for (int i = 0; i < 14; i++)
        {
            cardPoints[i] = this.gameObject.transform.GetChild(i).gameObject;
        }
        plusCardPosition = cardPoints[13].transform;
        return cardPoints;
    }

    public void AddCard(CardComponent newPlusCard) 
    {
        cards[13] = newPlusCard;
        cards[13].SetPosition(plusCardPosition.position, plusCardPosition.rotation);
    }

    public void Discard(CardComponent stashCard)
    {
        for (var i = 0; i < 14; ++i)
        {
            if (cards[i] == stashCard)
            {
                var cardPoint = CardPoints[i];

                cards[i] = cards[13];
                cards[i].SetPosition(cardPoint.transform.position, cardPoint.transform.rotation);
                cards[13] = null;
            }
        }

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
        for (int i = 0; i< pickedCards.Length; ++i)
        {
            var pointTransform = CardPoints[i].transform;
            cards[i] = pickedCards[i];
            cards[i].SetPosition(pointTransform.position, pointTransform.transform.rotation); 
        }
        SortCard();
    }

    public void Pickupable(bool b)
    {
        foreach (CardComponent card in cards)
        {
            if (card != null)
            {
                card.gameObject.GetComponent<BoxCollider>().enabled = b;
            }
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