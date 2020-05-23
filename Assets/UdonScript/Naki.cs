
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Naki : UdonSharpBehaviour
{
    public bool canChi = false, canPon = false, canKkan = false;
    public GameObject nakiObject;
    NakiData[] nakiData;
    private int nakiDataCount = 0;

    public void Initialized()
    {
        nakiData = nakiObject.GetComponentsInChildren<NakiData>();
    }

    public void search(CardComponent[] cards, CardComponent newCard)
    {
        CardComponent[] addNewCard = new CardComponent[cards.Length + 1];
        int aNCCount = 0;
        
        foreach(CardComponent card in cards)
        {
            addNewCard[aNCCount++] = card;
        }
        addNewCard[addNewCard.Length-1] = newCard;

        // 0:±ø, 1:Ä¿Âê, 2:šœÂê
        int[] originCardNakiCount = findNakiable(cards);
        int[] addCardNakiCount = findNakiable(SortCard(addNewCard));

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
        int[] nakiableCount = new int[3] { 0, 0, 0 }; // 0:±ø, 1:Ä¿Âê, 2:šœÂê
        int kuzzCount = 0, shunzzCount = 0;
        CardComponent[] kuzz = new CardComponent[4];
        CardComponent[] shunzz = new CardComponent[4];

        foreach (CardComponent card in cards) // Ä¿Âê,±ø Ä«¿îÆ®
        {
            if (kuzzCount == 0 || shunzzCount == 0)
            {
                kuzz[kuzzCount++] = card;
                shunzz[shunzzCount++] = card;
            }
            else
            {
                if (kuzz[kuzzCount - 1].Type == card.Type)
                {
                    if (kuzz[kuzzCount - 1].CardNumber == card.CardNumber)
                    {
                        kuzz[kuzzCount] = card;
                    }
                    else if (kuzz[kuzzCount - 1].CardNumber == card.CardNumber - 1)
                    {
                        shunzz[shunzzCount] = card;
                    }
                }
            }
            if (kuzzCount == 3)
            {
                nakiableCount[1]++;
                nakiData[nakiDataCount++].Initialize("Ä¿Âê",kuzz);
                kuzzCount = 0;
            }
            else if (kuzzCount == 4)
            {
                nakiableCount[1]--;
                nakiableCount[0]++;
                nakiData[--nakiDataCount].Initialize("±øÁî", kuzz);
                kuzzCount = 0;
            }
            if (shunzzCount == 3)
            {
                nakiableCount[2]++;
                nakiData[nakiDataCount++].Initialize("šœÂê", shunzz);
                shunzzCount = 0;
            }
        }
        return nakiableCount;
    }
}
