
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Naki : UdonSharpBehaviour
{
    //CardComponent[,] pon = new CardComponent[4,3];
    //CardComponent[,] chi = new CardComponent[4, 3];
    //CardComponent[,] kkan = new CardComponent[4, 4];
    public bool canChi = false, canPon = false, canKkan = false;


    public void search(CardComponent[] cards, CardComponent newCard)
    {
        CardComponent[] addNewCard = new CardComponent[cards.Length + 1];
        
        foreach(CardComponent card in cards)
        {
            addNewCard[addNewCard.Length] = card;
        }
        addNewCard[addNewCard.Length] = newCard;

        // 0:±ø, 1:Ä¿Âê, 2:šœÂê
        int[] originCardNakiCount = findNakiable(cards);
        int[] addCardNakiCount = findNakiable(addNewCard);

        for (var i = 0; i < 3; i++)
        {
            if(originCardNakiCount[i] != addCardNakiCount[i])
            {
                switch (i) 
                {
                    case 0:
                        canKkan = true;
                        break;
                    case 1:
                        canPon = true;
                        break;
                    case 2:
                        canChi = true;
                        break;
                }
            }
        }

    }

    public CardComponent[] SortCard(CardComponent[] cards)
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

    private int[] findNakiable(CardComponent[] cards)
    {
        //int kkanCount = 0, kuzzCount = 0, shunzzCount = 0;
        int[] nakiableCount = new int[3] { 0, 0, 0 }; // 0:±ø, 1:Ä¿Âê, 2:šœÂê
        CardComponent[] kuzz = new CardComponent[4];
        CardComponent[] shunzz = new CardComponent[4];
        foreach (CardComponent card in cards) // Ä¿Âê,±ø Ä«¿îÆ®
        {
            if (kuzz.Length == 0 || shunzz.Length == 0)
            {
                kuzz[kuzz.Length] = card;
            }
            else
            {
                if (kuzz[kuzz.Length - 1].Type == card.Type)
                {
                    if (kuzz[kuzz.Length - 1].CardNumber == card.CardNumber)
                    {
                        kuzz[kuzz.Length] = card;
                    }
                    else if (kuzz[kuzz.Length - 1].CardNumber == card.CardNumber - 1)
                    {
                        shunzz[shunzz.Length] = card;
                    }
                }
            }
            if (kuzz.Length == 3)
            {
                nakiableCount[1]++;
            }
            else if (kuzz.Length == 4)
            {
                nakiableCount[1]--;
                nakiableCount[0]++;
            }
            if (shunzz.Length == 3)
            {
                nakiableCount[2]++;
            }
        }
        return nakiableCount;
    }
}
