
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandCalculator : UdonSharpBehaviour
{
    public DebugHelper DebugHelper;
    public Card[] TestComponents;

    const int TILES_COUNT = 34;

    public KList Stack;
    public KList Result;
    public CalculatingContextHandler Ctx;

    public HandUtil HandUtil;
    public Chiitoitsu Chiitoitsu;
    public Kokushimusou Kokushimusou;

    // NOTE) 슌쯔, 커쯔를 영어로 쓰기가 귀찮고 길어서 Chi, Pon으로 줄여서 씀

    public void RequestNakiable(Card[] cards, UIContext uiContext, AgariContext agariContext, Card discardedCard)
    {
        var chiableList = GetChiableAll(cards, discardedCard);
        var isChiable = chiableList.Length > 0;

        uiContext.IsChiable = isChiable;
        uiContext.IsPonable = IsPonable(cards, discardedCard);
        uiContext.IsKkanable = IsKkanable(cards, discardedCard);

        SetChiableOnUiContext(uiContext, chiableList);
        // TODO;
    }

    void SetChiableOnUiContext(UIContext uiContext, object[] chiableList)
    {
        var chiableCount = chiableList.Length;

        uiContext.ChiableCount = chiableCount;

        for (var i = 0; i< chiableCount; ++i)
        {
            var chiableCards = (Card[])chiableList[i];
            var indexes = new int[2];
            var cardSpriteNames = new string[2];

            for (var cardIndex = 0; cardIndex < chiableCards.Length; ++cardIndex) 
            {
                var card = chiableCards[cardIndex];
                indexes[cardIndex] = card.Index;
                cardSpriteNames[cardIndex] = card.GetCardSpriteName();
            }

            switch (i)
            {
                case 0:
                    uiContext.ChiableIndex1 = new Vector2(indexes[0], indexes[1]);
                    uiContext.ChiableSprite11 = cardSpriteNames[0];
                    uiContext.ChiableSprite12 = cardSpriteNames[1];
                    break;

                case 1:
                    uiContext.ChiableIndex2 = new Vector2(indexes[0], indexes[1]);
                    uiContext.ChiableSprite21 = cardSpriteNames[0];
                    uiContext.ChiableSprite22 = cardSpriteNames[1];
                    break;

                case 2:
                    uiContext.ChiableIndex3 = new Vector2(indexes[0], indexes[1]);
                    uiContext.ChiableSprite31 = cardSpriteNames[0];
                    uiContext.ChiableSprite32 = cardSpriteNames[1];
                    break;
            }
        }
    }

    public object[] GetChiableAll(Card[] cards, Card discardedCard)
    {
        // 자패의 슌쯔는 검사하지 않는다
        var cardOrder = discardedCard.GlobalOrder;
        if (cardOrder >= HandUtil.GetWordsStartGlobalOrder()) { return new object[0]; }

        var globalOrders = HandUtil.GetGlobalOrders(cards);

        // 같은 type의 패만 슌쯔를 검사한다
        var startOrder = HandUtil.GetStartGlobalOrderOf(discardedCard.Type);
        var endOrder = HandUtil.GetEndGlobalOrderOf(discardedCard.Type);

        var list = new object[3];
        var count = 0;

        // for문으로 바꾸려는 시도를 해봤는데, 이거보다 보기 더 어려워져서 그냥 이렇게 함
        if (startOrder + 2 <= cardOrder && cardOrder <= endOrder - 0 && globalOrders[cardOrder - 2] > 0 && globalOrders[cardOrder - 1] > 0)
        {
            list[count++] = ToCards(cards, new int[] { cardOrder - 2, cardOrder - 1 });
        }
        if (startOrder + 1 <= cardOrder && cardOrder <= endOrder - 1 && globalOrders[cardOrder - 1] > 0 && globalOrders[cardOrder + 1] > 0)
        {
            list[count++] = ToCards(cards, new int[] { cardOrder - 1, cardOrder + 1});
        }
        if (startOrder + 0 <= cardOrder && cardOrder <= endOrder - 2 && globalOrders[cardOrder + 1] > 0 && globalOrders[cardOrder + 2] > 0)
        {
            list[count++] = ToCards(cards, new int[] { cardOrder + 1, cardOrder +2 });
        }

        // return object[ Card[], Card[], ... ]
        return Fit(list, count);
    }

    public bool IsPonable(Card[] cards, Card discardedCard)
    {
        var globalOrders = HandUtil.GetGlobalOrders(cards);
        var cardOrder = discardedCard.GlobalOrder;

        return globalOrders[cardOrder] + 1 >= 3;
    }

    public bool IsKkanable(Card[] cards, Card discardedCard)
    {
        var globalOrders = HandUtil.GetGlobalOrders(cards);
        var cardOrder = discardedCard.GlobalOrder;

        return globalOrders[cardOrder] + 1 == 4;
    }

    public int[] IsRiichiable(Card[] cards)
    {
        if (cards.Length != 14) Debug.Log("cards.Length != 14");

        // 버려서 텐파이 되는 곳을 찾는다
        var riichiCreationOrders = new int[TILES_COUNT];

        var globalOrders = HandUtil.GetGlobalOrders(cards);
        for (var i = 0; i < globalOrders.Length; ++i)
        {
            if (globalOrders[i] > 0)
            {
                --globalOrders[i];
                if (IsTenpai(globalOrders))
                {
                    riichiCreationOrders[i] = 1;
                }
                ++globalOrders[i];
            }
        }

        return riichiCreationOrders;
    }

    public bool IsTenpai(int[] globalOrders)
    {
        // 머리가 6개인가?
        if (Chiitoitsu.IsTenpai(globalOrders))
        {
            return true;
        }

        // 요구패가 13개인가?
        if (Kokushimusou.IsTenpai(globalOrders))
        {
            return true;
        }

        // check normal
        return true;
    }

    object[] FindAll(int[] globalOrders, int[] openedGlobalOrders)
    {
        if (globalOrders.Length != TILES_COUNT) Debug.Log("TILES LENGTH ERROR!");

        Result.Clear();

        var maxChiPonCount = 0;
        foreach (var pair in HandUtil.FindPairs(globalOrders))
        {
            var localGlobalOrders = Clone(globalOrders);
            localGlobalOrders[pair] -= 2;

            var manCtxs = Find(localGlobalOrders, HandUtil.GetManStartGlobalOrder(), HandUtil.GetManEndGlobalOrder());
            var pinCtxs = Find(localGlobalOrders, HandUtil.GetPinStartGlobalOrder(), HandUtil.GetPinEndGlobalOrder());
            var souCtxs = Find(localGlobalOrders, HandUtil.GetSouStartGlobalOrder(), HandUtil.GetSouEndGlobalOrder());
            var wordCtxs = Find(localGlobalOrders, HandUtil.GetWordsStartGlobalOrder(), HandUtil.GetWordsEndGlobalOrder());

            // 하나도 없으면 foreach문 자체에 안 들어가서
            // 공집합을 표현해주기 위해 null 하나 넣음
            manCtxs = Ctx.AddNullForward(manCtxs);
            pinCtxs = Ctx.AddNullForward(pinCtxs);
            souCtxs = Ctx.AddNullForward(souCtxs);
            wordCtxs = Ctx.AddNullForward(wordCtxs);

            // 111222333의 경우, 123/123/123과 111/222/333이 있을 수 있다
            // 따라서 (만의 슌커쯔)x(삭의 슌커쯔)x(통의 슌커쯔)가 경우의 수가 된다
            // 자패는 커쯔밖에 없으니 바뀔 일이 없음
            foreach (object[] manCtx in manCtxs)
            {
                foreach (object[] pinCtx in pinCtxs)
                {
                    foreach (object[] souCtx in souCtxs)
                    {
                        foreach (object[] wordCtx in wordCtxs)
                        {
                            var ctx = Ctx.AddContextAll(manCtx, pinCtx, souCtx, wordCtx);
                            if (ctx == null) { continue; }

                            var chiPonCount = Ctx.ReadChiCount(ctx) + Ctx.ReadPonCount(ctx);
                            if (maxChiPonCount <= chiPonCount)
                            {
                                var t = Ctx.ReadGlobalOrders(ctx);
                                t[pair] += 2;

                                if (maxChiPonCount < chiPonCount)
                                {
                                    maxChiPonCount = chiPonCount;
                                    Result.Clear();
                                }

                                Result.Add(ctx);
                            }
                        }
                    }
                }
            }
        }

        return Result.Clone();
    }

    // context[0]: int[38] remainTiles
    // context[1]: object[int[3]] chis
    // context[2]: int chiCount
    // context[3]: object[int[3]] pons
    // context[4]: int ponCount

    object[] Find(int[] originalGlobalOrders, int startOrder, int endOrder)
    {
        // 한개도 안 되면 리턴하지 말기
        var maxChiPonCount = 1;
        var first = Ctx.CreateContext(originalGlobalOrders);

        Stack.Clear();
        Result.Clear();

        Stack.Add(first);

        while (Stack.Count() != 0)
        {
            var top = (object[])Stack.RemoveLast();
            var globalOrders = Ctx.ReadGlobalOrders(top);
            var chiCount = Ctx.ReadChiCount(top);
            var ponCount = Ctx.ReadPonCount(top);
            var isChanged = false;

            // 자패의 슌쯔는 검사하지 않는다
            if (startOrder < HandUtil.GetWordsStartGlobalOrder())
            {
                for (var i = startOrder; i <= endOrder - 2; ++i)
                {
                    // 치는 뭐부터 먼저 하는지에 따라 순서가 갈려서 각 경우 전부 검사해야 함 
                    // 11234의 경우, (123)(14)와 (11)(234)가 가능하다
                    if (CanChi(globalOrders, i))
                    {
                        var context = Ctx.CopyContext(top);
                        Ctx.ApplyChi(context, i);

                        if (!Ctx.HasSameGlobalOrders(Stack.Clone(), context))
                        {
                            Stack.Add(context);
                        }

                        isChanged = true;

                        // 123 456에서 123이 제거된 경우, 123의 남은 갯수는 000임
                        // 456부터 세는 것과 동치이기 때문에 스킵함

                        // 1233 456일때 234를 제거한 경우 234의 남은 갯수는 010임
                        // 이 경우는 다른 경우를 검사해보아야 함 (실제로 최적해는 123,3,456이다)
                        if (Ctx.IsRemainsChiCountEqual(context, i))
                        {
                            break;
                        }
                    }
                }
            }

            for (var order = startOrder; order <= endOrder; ++order)
            {
                if (CanPon(globalOrders, order))
                {
                    var context = Ctx.CopyContext(top);
                    Ctx.ApplyPon(context, order);

                    if (!Ctx.HasSameGlobalOrders(Stack.Clone(), context))
                    {
                        Stack.Add(context);
                    }

                    isChanged = true;

                    // 퐁은 순서가 바뀌어도 딱히 영향을 받지 않는다
                    // 333 444 555에서 333을 먼저 제거해도 444 555가 깨지지 않는다
                    break;
                }
            }

            // chi, pon 둘 다 안되는 경우
            // Result에 넣는 검사를 하게 된다
            if (!isChanged)
            {
                var chiPonCount = chiCount + ponCount;
                if (maxChiPonCount < chiPonCount)
                {
                    maxChiPonCount = chiPonCount;

                    // 더 많은 슌커쯔를 가진 조합이 나오면 이전 조합을 모두 버림
                    Result.Clear();
                    Result.Add(top);
                }
                else if (maxChiPonCount == chiPonCount)
                {
                    // unique한 결과값만 넣도록 함
                    if (!Ctx.HasSameChiPons(Result.Clone(), top))
                    {
                        Result.Add(top);
                    }
                }
            }
        }

        return Result.Clone();
    }

    bool CanPon(int[] globalOrders, int cardGlobalOrder)
    {
        return globalOrders[cardGlobalOrder] > 2;
    }

    bool CanChi(int[] globalOrders, int cardGlobalOrder)
    {
        if (cardGlobalOrder + 2 >= globalOrders.Length) { return false; }
        return globalOrders[cardGlobalOrder] > 0
            && globalOrders[cardGlobalOrder + 1] > 0
            && globalOrders[cardGlobalOrder + 2] > 0;
    }

    int[] Clone(int[] arr)
    {
        var clone = new int[arr.Length];
        for (var i = 0; i < arr.Length; ++i)
        {
            clone[i] = arr[i];
        }
        return clone;
    }

    Card[] ToCards(Card[] cards, int[] cardGlobalOrders)
    {
        var findCount = cardGlobalOrders.Length;
        var findIndex = 0;
        var arr = new Card[findCount];

        foreach (var card in cards)
        {
            if (card.GlobalOrder == cardGlobalOrders[findIndex])
            {
                arr[findIndex] = card;
                findIndex++;

                if (findIndex == findCount) { return arr; }
            }
        }

        return arr;
    }

    void Test1()
    {
        DebugHelper.SetTestName("Test1");

        var testSet = new Card[]
        {
            TEST__SetTestData(TestComponents[0], "만", 1),
            TEST__SetTestData(TestComponents[1], "만", 1),
            TEST__SetTestData(TestComponents[2], "만", 2),
            TEST__SetTestData(TestComponents[3], "만", 2),
            TEST__SetTestData(TestComponents[4], "만", 3),
            TEST__SetTestData(TestComponents[5], "만", 3),
        };

        var globalOrders = HandUtil.GetGlobalOrders(testSet);
        var manCtxs = Find(globalOrders, HandUtil.GetManStartGlobalOrder(), HandUtil.GetManEndGlobalOrder());

        DebugHelper.Equal(manCtxs.Length, 1, 1);

        // (1,2,3) (1,2,3)
        DebugHelper.Equal(Ctx.TEST__GetChiCount(manCtxs, 0, 0), 2, 2);
    }

    void Test2()
    {
        DebugHelper.SetTestName("Test2");

        var testSet = new Card[]
          {
                    TEST__SetTestData(TestComponents[0], "만", 1),
                    TEST__SetTestData(TestComponents[1], "만", 1),
                    TEST__SetTestData(TestComponents[2], "만", 1),
                    TEST__SetTestData(TestComponents[3], "만", 2),
                    TEST__SetTestData(TestComponents[4], "만", 2),
                    TEST__SetTestData(TestComponents[5], "만", 2),
                    TEST__SetTestData(TestComponents[6], "만", 3),
                    TEST__SetTestData(TestComponents[7], "만", 3),
                    TEST__SetTestData(TestComponents[8], "만", 3),
                    TEST__SetTestData(TestComponents[9], "만", 9),
      };

        var globalOrders = HandUtil.GetGlobalOrders(testSet);
        var manCtxs = Find(globalOrders, HandUtil.GetManStartGlobalOrder(), HandUtil.GetManEndGlobalOrder());

        DebugHelper.Equal(manCtxs.Length, 2, 1);

        // (1,1,1) (2,2,2) (3,3,3)
        DebugHelper.Equal(Ctx.TEST__GetPonCount(manCtxs, 0, 0), 1, 2);
        DebugHelper.Equal(Ctx.TEST__GetPonCount(manCtxs, 0, 1), 1, 3);
        DebugHelper.Equal(Ctx.TEST__GetPonCount(manCtxs, 0, 2), 1, 4);

        // (1,2,3) (1,2,3) (1,2,3)
        DebugHelper.Equal(Ctx.TEST__GetChiCount(manCtxs, 1, 0), 3, 5);
    }

    void Test3()
    {
        DebugHelper.SetTestName("Test3");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1),
                    TEST__SetTestData(TestComponents[1], "만", 1),
                    TEST__SetTestData(TestComponents[2], "만", 2),
                    TEST__SetTestData(TestComponents[3], "만", 2),
                    TEST__SetTestData(TestComponents[4], "만", 3),
                    TEST__SetTestData(TestComponents[5], "만", 3),
                    TEST__SetTestData(TestComponents[6], "만", 3),
                    TEST__SetTestData(TestComponents[7], "만", 4),
                    TEST__SetTestData(TestComponents[8], "만", 5),
                    TEST__SetTestData(TestComponents[9], "만", 6),
                    TEST__SetTestData(TestComponents[10], "만", 7),
                    TEST__SetTestData(TestComponents[11], "만", 8),
                    TEST__SetTestData(TestComponents[12], "만", 8),
                    TEST__SetTestData(TestComponents[13], "만", 8),
        };
        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        var resultCtxs = FindAll(globalOrders, null);

        // (1,2,3) (1,2,3) (3,4,5) (6,7,8)
        DebugHelper.Equal(resultCtxs.Length, 1, 1);

        DebugHelper.Equal(Ctx.TEST__GetChiCount(resultCtxs, 0, 0), 2, 2);
        DebugHelper.Equal(Ctx.TEST__GetChiCount(resultCtxs, 0, 2), 1, 3);
        DebugHelper.Equal(Ctx.TEST__GetChiCount(resultCtxs, 0, 5), 1, 4);
    }

    void Test4()
    {
        DebugHelper.SetTestName("Test4");

        var testSet = new Card[2]
        {
                    TEST__SetTestData(TestComponents[0], "만", 2),
                    TEST__SetTestData(TestComponents[1], "만", 3),
        };

        var chiTarget = TEST__SetTestData(TestComponents[2], "삭", 1);

        DebugHelper.IsFalse(TEST__IsChiable(testSet, chiTarget), 1);

        chiTarget = TEST__SetTestData(TestComponents[2], "만", 1);

        DebugHelper.IsTrue(TEST__IsChiable(testSet, chiTarget), 2);

        chiTarget = TEST__SetTestData(TestComponents[2], "만", 4);

        DebugHelper.IsTrue(TEST__IsChiable(testSet, chiTarget), 3);
    }

    void Test5()
    {
        DebugHelper.SetTestName("Test5");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 3),
                    TEST__SetTestData(TestComponents[1], "만", 3),
        };
        var ponTarget = TEST__SetTestData(TestComponents[2], "만", 3);
        DebugHelper.IsTrue(IsPonable(testSet, ponTarget), 1);
    }

    void Test6()
    {
        DebugHelper.SetTestName("Test6");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1),
                    TEST__SetTestData(TestComponents[1], "만", 1),
                    TEST__SetTestData(TestComponents[2], "만", 2),
                    TEST__SetTestData(TestComponents[3], "만", 2),
                    TEST__SetTestData(TestComponents[4], "만", 3),
                    TEST__SetTestData(TestComponents[5], "만", 3),
                    TEST__SetTestData(TestComponents[6], "만", 4),
                    TEST__SetTestData(TestComponents[7], "만", 4),
                    TEST__SetTestData(TestComponents[8], "만", 5),
                    TEST__SetTestData(TestComponents[9], "만", 5),
                    TEST__SetTestData(TestComponents[10], "만", 6),
                    TEST__SetTestData(TestComponents[11], "만", 6),
                    TEST__SetTestData(TestComponents[12], "만", 7),
                    TEST__SetTestData(TestComponents[13], "삭", 9),
        };
        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        DebugHelper.IsTrue(Chiitoitsu.IsTenpai(globalOrders), 1);

        TEST__SetTestData(TestComponents[13], "만", 7);
        globalOrders = HandUtil.GetGlobalOrders(testSet);

        DebugHelper.IsTrue(Chiitoitsu.IsWinable(globalOrders), 2);
    }

    void Test7()
    {
        DebugHelper.SetTestName("Test7");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1),
                    TEST__SetTestData(TestComponents[1], "만", 9),
                    TEST__SetTestData(TestComponents[2], "삭", 1),
                    TEST__SetTestData(TestComponents[3], "삭", 9),
                    TEST__SetTestData(TestComponents[4], "통", 1),
                    TEST__SetTestData(TestComponents[5], "통", 9),
                    TEST__SetTestData(TestComponents[6], "동", 1),
                    TEST__SetTestData(TestComponents[7], "남", 2),
                    TEST__SetTestData(TestComponents[8], "서", 3),
                    TEST__SetTestData(TestComponents[9], "북", 4),
                    TEST__SetTestData(TestComponents[10], "백", 5),
                    TEST__SetTestData(TestComponents[11], "발", 6),
                    TEST__SetTestData(TestComponents[12], "발", 6),
                    TEST__SetTestData(TestComponents[13], "만", 1),
        };
        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        DebugHelper.IsTrue(Kokushimusou.IsTenpai(globalOrders), 1);

        TEST__SetTestData(TestComponents[13], "중", 7);
        globalOrders = HandUtil.GetGlobalOrders(testSet);

        DebugHelper.IsTrue(Kokushimusou.IsWinable(globalOrders), 2);
    }

    void Test8()
    {
        DebugHelper.SetTestName("Test8");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "동", 1),
                    TEST__SetTestData(TestComponents[1], "서", 2),
        };
        var chiTarget = TEST__SetTestData(TestComponents[2], "남", 3);

        // 동서남은 순서상 123이긴 한데 chi 가능하지 않음
        DebugHelper.IsFalse(TEST__IsChiable(testSet, chiTarget), 1);
    }

    void Test9()
    {
        DebugHelper.SetTestName("Test9");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1),
                    TEST__SetTestData(TestComponents[1], "만", 2),
                    TEST__SetTestData(TestComponents[2], "만", 3),
                    TEST__SetTestData(TestComponents[3], "만", 4),
        };
        var chiTarget = TEST__SetTestData(TestComponents[4], "만", 2);

        // (1,2,3) (2,3,4)가 가능함)
        var chiables = GetChiableAll(testSet, chiTarget);
        DebugHelper.Equal(chiables.Length, 2, 1);
    }

    public void Start()
    {
        if (DebugHelper == null) { return; }

        DebugHelper.SetClassName("HandCalculator");

        TestComponents = GetComponentsInChildren<Card>();

        Test1();
        Test2();
        Test3();
        Test4();
        Test5();
        Test6();
        Test7();
        Test8();
        Test9();
    }

    public bool TEST__IsChiable(Card[] cards, Card discardedCard)
    {
        return GetChiableAll(cards, discardedCard).Length != 0;
    }

    Card TEST__SetTestData(Card card, string type, int cardNumber)
    {
        card.Type = type;
        card.CardNumber = cardNumber;
        card.GlobalOrder = HandUtil.GetGlobalOrder(type, cardNumber);
        return card;
    }

    public object[] Fit(object[] arr, int count)
    {
        var newArr = new object[count];
        for (var i = 0; i < count; ++i)
        {
            newArr[i] = arr[i];
        }
        return newArr;
    }
}