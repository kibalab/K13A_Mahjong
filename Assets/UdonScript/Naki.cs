
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Naki : UdonSharpBehaviour
{
    public bool IgnoreTests = false;

    public bool canChi = false, canPon = false, canKkan = false;
    public GameObject nakiObject;
    public NakiData[] nakiData;
    private int nakiDataCount = 0;


    //test variables//
    public CardComponent[] TestComponents;

    private void Start()
    {
        if (IgnoreTests) { return; }

        Initialized(null);

        TestComponents = GetComponentsInChildren<CardComponent>();

        Debug.Log("--- Naki TEST ---");
        

        Debug.Log("--- TEST Level0-0 ---");
        var testSet = new CardComponent[]
        {
                TEST__SetTestData(TestComponents[0], "만", 3, 2),
                TEST__SetTestData(TestComponents[1], "만", 3, 2),
                TEST__SetTestData(TestComponents[2], "만", 3, 2)
        };
        var testSetNew_ = TEST__SetTestData(TestComponents[2], "만", 3, 2);
        canPon = nakiData[0].checkCanNaki("kuzz", GetHardCopy(testSet), testSetNew_);
        Debug.Log(canPon);


        Debug.Log("--- TEST Level0-1 ---");
        testSet = new CardComponent[]
        {
                TEST__SetTestData(TestComponents[0], "만", 3, 2),
                TEST__SetTestData(TestComponents[1], "만", 3, 2),
                TEST__SetTestData(TestComponents[2], "만", 3, 2),
                TEST__SetTestData(TestComponents[3], "만", 3, 2)
        };
        testSetNew_ = TEST__SetTestData(TestComponents[2], "만", 3, 2);
        canPon = nakiData[0].checkCanNaki("kuzz+", GetHardCopy(testSet), testSetNew_);
        Debug.Log(canPon);


        Debug.Log("--- TEST Level0-2 ---");
        testSet = new CardComponent[]
        {
                TEST__SetTestData(TestComponents[0], "만", 3, 2),
                TEST__SetTestData(TestComponents[1], "만", 4, 3),
                TEST__SetTestData(TestComponents[2], "만", 5, 4)
        };
        testSetNew_ = TEST__SetTestData(TestComponents[1], "만", 4, 3);
        canPon = nakiData[0].checkCanNaki("shunzz", GetHardCopy(testSet), testSetNew_);
        Debug.Log(canPon);


        Debug.Log("--- TEST Level1 ---");
        testSet = new CardComponent[]
        {
                TEST__SetTestData(TestComponents[0], "만", 3, 2),
                TEST__SetTestData(TestComponents[1], "만", 3, 2),
                TEST__SetTestData(TestComponents[2], "만", 3, 2),
                TEST__SetTestData(TestComponents[3], "만", 4, 3),
                TEST__SetTestData(TestComponents[4], "만", 5, 4),
                TEST__SetTestData(TestComponents[5], "만", 6, 5),
                TEST__SetTestData(TestComponents[6], "만", 7, 6),
                TEST__SetTestData(TestComponents[7], "만", 8, 7),
                TEST__SetTestData(TestComponents[8], "만", 9, 8),
                TEST__SetTestData(TestComponents[9], "삭", 1, 9),
                TEST__SetTestData(TestComponents[10], "삭", 2, 10),
                TEST__SetTestData(TestComponents[11], "삭", 2, 10),
                TEST__SetTestData(TestComponents[12], "삭", 3, 11),
                TEST__SetTestData(TestComponents[13], "삭", 4, 12)
        };
        var testSetNew = TEST__SetTestData(TestComponents[1], "만", 3, 2);
        var testSetNew1 = TEST__SetTestData(TestComponents[5], "만", 6, 5);
        var testSetNew2 = TEST__SetTestData(TestComponents[10], "삭", 2, 10);

        findShunzz_Test(testSet, testSetNew);
        findShunzz_Test(testSet, testSetNew1);
        findShunzz_Test(testSet, testSetNew2);
        
    }

    CardComponent TEST__SetTestData(CardComponent card, string type, int cardNumber, int normalCardNumber)
    {
        card.Type = type;
        card.CardNumber = cardNumber;
        card.NormalCardNumber = normalCardNumber;
        return card;
    }

    public void Initialized(EventQueue e)
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
        int nakiTop = 0;

        CardComponent lastShunzzCard = null;
        CardComponent lastKuzzCard = null;

        foreach (CardComponent card in addNewCard)
        {
            Debug.Log("addNewCards : " + card.CardNumber + card.Type);
        }

        for (var i = 0; i < 14; i++)
        {
            CardComponent[] shunzzCardStack = new CardComponent[3];
            CardComponent[] kuzzCardStack = new CardComponent[4];
            int shunzzStackTop = -1, kuzzStackTop = -1;
            Debug.Log(addNewCard.Length);
            Debug.Log("TestCardLevel : " + i + "/" + (0) + ", " + addNewCard[i].CardNumber + addNewCard[i].Type + ", " + addNewCard[i].NormalCardNumber);
            shunzzCardStack[++shunzzStackTop] = addNewCard[i];
            kuzzCardStack[++kuzzStackTop] = addNewCard[i];
            for (var j = 0; j < 13 - i; j++)
            {
                if (true) //lastShunzzCard == null || lastShunzzCard.NormalCardNumber != addNewCard[i + j].NormalCardNumber
                {
                    if (shunzzCardStack[shunzzStackTop].CardNumber + 1 == addNewCard[i + j].CardNumber && shunzzCardStack[shunzzStackTop].Type == addNewCard[i + j].Type)
                    {
                        if (shunzzStackTop <= 1)
                        {
                            Debug.Log("TestCardLevel : " + i + "/" + j + ", " + addNewCard[i + j].CardNumber + addNewCard[i + j].Type + ", " + addNewCard[i + j].NormalCardNumber);
                            shunzzCardStack[++shunzzStackTop] = addNewCard[i + j];
                            //if (shunzzStackTop >= 2) break;
                        }
                    }
                    if (kuzzCardStack[kuzzStackTop].CardNumber == addNewCard[i + j].CardNumber && kuzzCardStack[kuzzStackTop].Type == addNewCard[i + j].Type && kuzzCardStack[kuzzStackTop] != addNewCard[i + j])
                    {
                        if (kuzzStackTop <= 3)
                        {
                            Debug.Log("TestCardLevel : " + i + "/" + j + ", " + addNewCard[i + j].CardNumber + addNewCard[i + j].Type + ", " + addNewCard[i + j].NormalCardNumber);
                            kuzzCardStack[++kuzzStackTop] = addNewCard[i + j];
                            //if (kuzzStackTop >= 3) break;
                        }
                    }
                }
            }
            if(shunzzStackTop >= 2)
            {
                //Shunzz[shunzzSize] = GetHardCopy(cardStack);
                Debug.Log("nakiDataTop : " + nakiTop);
                Debug.Log("nakiDataStorageSize : " + nakiData.Length);
                canChi = nakiData[nakiTop].checkCanNaki("shunzz", GetHardCopy(shunzzCardStack), newCard);
                nakiTop = canChi ? nakiTop+1 : nakiTop;
                nakiDataCount = nakiTop;
                lastShunzzCard = addNewCard[i];
                Debug.Log("TestShunzzGroupSize : " + shunzzStackTop);
                for (var k = 0; k <= shunzzStackTop; k++)
                {
                    Debug.Log("TestShunzzGroup : " + shunzzCardStack[k].CardNumber + shunzzCardStack[k].Type);
                }
            }
            if (kuzzStackTop == 2)
            {
                Debug.Log(nakiTop);
                canPon = nakiData[nakiTop].checkCanNaki("kuzz", GetHardCopy(kuzzCardStack), newCard);
                nakiTop = canPon ? nakiTop + 1 : nakiTop;
                nakiDataCount = nakiTop;
                lastKuzzCard = addNewCard[i];
                Debug.Log("TestKuzzGroupSize : " + kuzzStackTop);
                for (var k = 0; k <= kuzzStackTop; k++)
                {
                    Debug.Log("TestKuzzGroup : " + kuzzCardStack[k].CardNumber + kuzzCardStack[k].Type);
                }
            }
            else if (kuzzStackTop == 3)
            {
                canKkan = nakiData[--nakiTop].checkCanNaki("kuzz+", GetHardCopy(kuzzCardStack), newCard);
                nakiTop = canKkan ? nakiTop + 1 : nakiTop;
                nakiDataCount = nakiTop;
                lastKuzzCard = addNewCard[i];
                Debug.Log("TestKuzzGroupSize : " + kuzzStackTop);
                for (var k = 0; k <= kuzzStackTop; k++)
                {
                    Debug.Log("TestKuzzGroup : " + kuzzCardStack[k].CardNumber + kuzzCardStack[k].Type);
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
    /*
    public void search(CardComponent[] cards, CardComponent newCard)
    {
        CardComponent[] addNewCard = new CardComponent[cards.Length + 1];
        int aNCCount = 0;
        
        foreach(CardComponent card in cards)
        {
            addNewCard[aNCCount++] = card;
        }
        addNewCard[addNewCard.Length-1] = newCard;

        // 0:깡, 1:커쯔, 2:슌쯔
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
                        newCard.EventType = "Kkan";
                        break;
                    case 1:
                        canPon = true;
                        newCard.EventType = "Pon";
                        break;
                    case 2:
                        canChi = true;
                        newCard.EventType = "Chi";
                        break;
                }
            }
        }

    }
    */
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
        int[] nakiableCount = new int[3] { 0, 0, 0 }; // 0:깡, 1:커쯔, 2:슌쯔
        int kuzzCount = 0, shunzzCount = 0;
        CardComponent[] kuzz = new CardComponent[4];
        CardComponent[] shunzz = new CardComponent[4];

        foreach (CardComponent card in cards) // 커쯔,깡 카운트
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
                //nakiData[nakiDataCount++].checkDifferentAndSaveData("커쯔",kuzz);
                kuzzCount = 0;
            }
            else if (kuzzCount == 4)
            {
                nakiableCount[1]--;
                nakiableCount[0]++;
                //nakiData[--nakiDataCount].checkDifferentAndSaveData("깡즈", kuzz);
                kuzzCount = 0;
            }
            if (shunzzCount == 3)
            {
                nakiableCount[2]++;
                //nakiData[nakiDataCount++].checkDifferentAndSaveData("슌쯔", shunzz);
                shunzzCount = 0;
            }
        }
        return nakiableCount;
    }
}
