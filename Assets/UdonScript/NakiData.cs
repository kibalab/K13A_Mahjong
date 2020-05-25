
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NakiData : UdonSharpBehaviour
{

    public bool nakiAble = false;
    public string Type;
    public CardComponent[] chiCards;
    public int id;



    public void Initialize(int id)
    {
        this.id = id;
    }

    public bool checkCanNaki(string type, CardComponent[] cards, CardComponent newCard)
    {
        Debug.Log("addNewCards : " + newCard.CardNumber + newCard.Type);
        foreach (CardComponent card in cards)
        {
            if (card != null)
            {
                Debug.Log("NakiCardLevel : " + card.CardNumber + card.Type + ", " + card.NormalCardNumber);
                if (card.CardNumber == newCard.CardNumber && card.Type == newCard.Type)
                {
                    nakiAble = true;
                    Type = type;
                    chiCards = cards;
                    return nakiAble;
                }
            }
        }

        return nakiAble;
    }
}
