using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardManager : UdonSharpBehaviour
{
    public string positionName;
    private int cardCount = 13;
    public CardComponent[] cards;
    public GameObject[] cardPoint = new GameObject[14];
    void Start()
    {
        findPoints();
    }
    GameObject[] findPoints()
    {
        //배열의 0~13 은 소유카드 14는 추가카드
        for (int i = 0; i <= cardCount; i++)
        {
            Debug.Log(this.gameObject.transform.GetChild(i).name);
            cardPoint[i] = this.gameObject.transform.GetChild(i).gameObject;
        }
        return cardPoint;
    }

    public void setCards()
    {
        for(int i = 0; i<= cardCount; i++)
        {
            cards[i].SetPosition(cardPoint[i].transform.position, cardPoint[i].transform.rotation); 
        }
    }

    public void Pickupable(bool b)
    {
        foreach (CardComponent card in cards)
        {
            card.gameObject.GetComponent<BoxCollider>().enabled = b;
        }
    }
    public CardComponent[] sortCard()
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