using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardManager : UdonSharpBehaviour
{
    private int cardCount = 13;
    public GameObject[] cards = new GameObject[13];
    public GameObject[] cardSprites = new GameObject[13];
    void Start()
    {
        findCards();
        Pickupable(false);
    }

    void findCards()
    {
        //배열의 0~13 은 소유카드 14는 추가카드
        for (int i = 0; i <= cardCount; i++)
        {
            cards[i] = this.gameObject.transform.GetChild(i).gameObject;
        }
    }

    public void Pickupable(bool b)
    {
        foreach (GameObject card in cards)
        {
            card.GetComponent<BoxCollider>().enabled = b;
        }
    }
    public void SetCard(string name, int index)
    {
    }
}
