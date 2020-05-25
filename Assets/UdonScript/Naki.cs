
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

        var i = 0;
        foreach(NakiData n in nakiData)
        {
            n.Initialize(i++);
        }
    }

    public void findShunzz_Test(CardComponent[] cards, CardComponent newCard)
    {
        CardComponent[] addNewCard = new CardComponent[cards.Length];
        int aNCCount = 0;

        foreach (CardComponent card in cards)
        {
            addNewCard[aNCCount++] = card;
        }
        addNewCard[addNewCard.Length - 1] = newCard;

        addNewCard = Sort(addNewCard);

        //object[] Shunzz = new object[20];
        int shunzzSize = 0;

        CardComponent lastShunzzCard = null;

        foreach (CardComponent card in addNewCard)
        {
            Debug.Log("addNewCards : " + card.CardNumber + card.Type);
        }

        for (var i = 0; i < 14; i++)
        {
            CardComponent[] cardStack = new CardComponent[4];
            int stackTop = -1;
            Debug.Log("TestCardLevel : " + i + "/" + (0) + ", " + addNewCard[i].CardNumber + addNewCard[i].Type + ", " + addNewCard[i].NormalCardNumber);
            cardStack[++stackTop] = addNewCard[i];
            for (var j = 0; j < 13 - i; j++)
            {
                if (true) //lastShunzzCard == null || lastShunzzCard.NormalCardNumber != addNewCard[i + j].NormalCardNumber
                {
                    if (cardStack[stackTop].CardNumber + 1 == addNewCard[i + j].CardNumber && cardStack[stackTop].Type == addNewCard[i + j].Type)
                    {
                        Debug.Log("TestCardLevel : " + i + "/" + j + ", " + addNewCard[i + j].CardNumber + addNewCard[i + j].Type + ", " + addNewCard[i + j].NormalCardNumber);
                        cardStack[++stackTop] = addNewCard[i + j];
                        if (stackTop >= 2) break;
                    }
                }
            }
            if(stackTop >= 2)
            {
                //Shunzz[shunzzSize] = GetHardCopy(cardStack);
                canChi = nakiData[shunzzSize].checkCanChi("shunzz", GetHardCopy(cardStack), newCard);
                shunzzSize = canChi ? shunzzSize+1 : shunzzSize;
                lastShunzzCard = addNewCard[i];
                Debug.Log("TestShunzzGroupSize : " + stackTop);
                for (var k = 0; k <= stackTop; k++)
                {
                    Debug.Log("TestShunzzGroup : " + cardStack[k].CardNumber + cardStack[k].Type);
                }
            }
        }
    }

    

    CardComponent[] Sort(CardComponent[] cards)
    {


        CardComponent temp;

        for (var i = cards.Length - 1; i >= 0; i--)
        {
            for (var j = 1; j <= i; j++)
            {
                if (cards[j - 1].NormalCardNumber > cards[j].NormalCardNumber)
                {
                    temp = cards[j - 1];
                    cards[j - 1] = cards[j];
                    cards[j] = temp;
                }
            }
        }

        return cards;
    }

    CardComponent[] GetHardCopy(CardComponent[] cards)
    {
        var copies = new CardComponent[cards.Length];
        for (var i = 0; i < cards.Length; ++i)
        {
            copies[i] = cards[i];
        }
        return copies;
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
                //nakiData[nakiDataCount++].checkDifferentAndSaveData("Ä¿Âê",kuzz);
                kuzzCount = 0;
            }
            else if (kuzzCount == 4)
            {
                nakiableCount[1]--;
                nakiableCount[0]++;
                //nakiData[--nakiDataCount].checkDifferentAndSaveData("±øÁî", kuzz);
                kuzzCount = 0;
            }
            if (shunzzCount == 3)
            {
                nakiableCount[2]++;
                //nakiData[nakiDataCount++].checkDifferentAndSaveData("šœÂê", shunzz);
                shunzzCount = 0;
            }
        }
        return nakiableCount;
    }
}
