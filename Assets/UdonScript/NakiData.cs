
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

    public bool checkCanChi(string type, CardComponent[] cards, CardComponent newCard)
    {
        var i = 0;
        Debug.Log("addNewCards : " + newCard.CardNumber + newCard.Type);
        foreach (CardComponent card in cards)
        {
            Debug.Log("ChiCardLevel : " + i + ", " + cards[i].CardNumber + cards[i].Type + ", " + cards[i].NormalCardNumber);
            if (cards[i].CardNumber == newCard.CardNumber && cards[i].Type == newCard.Type && (chiCards[0].CardNumber != newCard.CardNumber || chiCards[0].Type != newCard.Type)) 
            {
                nakiAble = true;
                Type = type;
                chiCards = cards;
                return nakiAble;
            }
            i++;
        }

        return nakiAble;
    }
}
