
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandCalculator : UdonSharpBehaviour
{
    public DebugHelper DebugHelper;

    // TODO 이거 테스트용으로 하나 넣어줘야 하는데, 씬 수정해야되서 나중에 
    public AgariContext AgariContextForTest;

    public Card[] TestComponents;

    const int TILES_COUNT = 34;

    public KList Stack;
    public KList Result;
    public CalculatingContextHandler Ctx;

    public HandUtil HandUtil;
    public Chiitoitsu Chiitoitsu;
    public Kokushimusou Kokushimusou;

    // NOTE) 슌쯔, 커쯔를 영어로 쓰기가 귀찮고 길어서 Chi, Pon으로 줄여서 씀

    public void RequestNakiable(Card[] cards, UIContext uiContext, AgariContext agariContext, Card discardedCard, bool isDiscardedByLeftPlayer)
    {
        var chiableList = GetChiableAll(cards, discardedCard);
        var isChiable = chiableList.Length > 0 && isDiscardedByLeftPlayer;

        uiContext.IsChiable = isChiable;
        uiContext.IsPonable = IsPonable(cards, discardedCard);
        uiContext.IsKkanable = IsKkanable(cards, discardedCard);

        if (isChiable)
        {
            SetChiableOnUiContext(uiContext, chiableList);
        }

        uiContext.IsRonable = agariContext.IsAgariable(discardedCard);
        uiContext.IsTsumoable = false;
    }

    void SetChiableOnUiContext(UIContext uiContext, object[] chiableList)
    {
        var chiableCount = chiableList.Length;

        uiContext.ChiableCount = chiableCount;

        for (var i = 0; i < chiableCount; ++i)
        {
            var chiableCards = (Card[])chiableList[i];
            var yamaIndexes = new int[2];
            var cardSpriteNames = new string[2];

            for (var cardIndex = 0; cardIndex < chiableCards.Length; ++cardIndex)
            {
                var card = chiableCards[cardIndex];
                yamaIndexes[cardIndex] = card.YamaIndex;
                cardSpriteNames[cardIndex] = card.GetCardSpriteName();
            }

            uiContext.SetChiable(i, yamaIndexes, cardSpriteNames);
        }
    }

    public object[] GetChiableAll(Card[] cards, Card discardedCard)
    {
        // 자패의 슌쯔는 검사하지 않는다
        var cardOrder = discardedCard.GlobalOrder;
        if (cardOrder >= HandUtil.GetWordsStartGlobalOrder()) { return new object[0]; }

        var targetCardType = discardedCard.Type;
        var targetGlobalOrder = discardedCard.GlobalOrder;
        var chiCandidates = new Card[4] { null, null, null, null };
        var chiDiff = new int[4] { -2, -1, +1, +2 };

        foreach (var card in cards)
        {
            // 버려진 카드와 같은 Type의 패만 슌쯔를 검사한다
            if (card.Type != targetCardType)
            {
                continue;
            }

            for (var i = 0; i < chiDiff.Length; ++i)
            {
                // 이미 후보가 있다면 검사할 필요가 없다
                if (chiCandidates[i] != null) { continue; }

                if (card.GlobalOrder == targetGlobalOrder + chiDiff[i])
                {
                    chiCandidates[i] = card;
                    break;
                }
            }
        }

        // 만1,2,3,4가 있는데 만3이 버려졌다면 아래와 같은 결과가 됨
        // chiCandidates[0] = 만1;
        // chiCandidates[1] = 만2;
        // chiCandidates[2] = 만4;
        // chiCandidates[3] = null;

        var list = new object[3];
        var count = 0;

        for (var i = 0; i < chiCandidates.Length - 1; ++i)
        {
            // 연속된 후보가 있다면 치가 가능한 것.
            // 위의 경우, (만1, 만2)와 (만2, 만4)가 가능
            var cand1 = chiCandidates[i];
            var cand2 = chiCandidates[i + 1];

            if (cand1 != null && cand2 != null)
            {
                list[count++] = new Card[] { cand1, cand2 };
            }
        }

        return Fit(list, count);
    }

    bool IsPonable(Card[] cards, Card discardedCard)
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

    public int[] GetAnkkanableAll(Card[] cards)
    {
        var globalOrders = HandUtil.GetGlobalOrders(cards);
        var ankkanableGlobalOrders = new int[10];
        var ankkanableGlobalOrderCount = 0;

        for (var i = 0; i< TILES_COUNT; ++i)
        {
            if (globalOrders[i] == 4)
            {
                ankkanableGlobalOrders[ankkanableGlobalOrderCount++] = i;
            }
        }

        return Fit_Int(ankkanableGlobalOrders, ankkanableGlobalOrderCount);
    }

    public bool IsOpenKkanable(Card addedCard, Card[] openedCards)
    {
        var globalOrders = HandUtil.GetGlobalOrders(openedCards);

        return globalOrders[addedCard.GlobalOrder] == 3;
    }

    public bool IsAnKkanable(Card[] cards)
    {
        var globalOrders = HandUtil.GetGlobalOrders(cards);

        foreach (var i in globalOrders)
        {
            if(i == 4)
            {
                return true;
            }
        }
        return false;
    }

    public void RequestRiichiable(Card[] sealedCards, AgariContext agariContext, UIContext uiContext)
    {
        var riichiableCards = GetRiichiableCards(agariContext, sealedCards);
        var isRiichiable = riichiableCards != null && riichiableCards.Length > 0;

        // [리치]를 누르면 별도 메뉴 없이 리치가 가능한 카드만 콜라이더를 살린다
        agariContext.RiichiCreationCards = riichiableCards;
        uiContext.IsRiichable = isRiichiable;
    }

    public Card[] GetRiichiableCards(AgariContext agariContext, Card[] cards)
    {
        if (cards.Length != 14)
        {
            Debug.Log("cards.Length != 14");
            return null;
        }

        // 버려서 텐파이 되는 곳을 찾는다
        var riichiableCards = new Card[cards.Length];
        var riichiableCount = 0;

        var globalOrders = HandUtil.GetGlobalOrders(cards);
        var prevGlobalOrder = -1;
        var prevIsRiichiCreationCard = false;

        foreach (var card in cards)
        {
            var isAlreadyCalculated = prevGlobalOrder == card.GlobalOrder;
            if (isAlreadyCalculated)
            {
                if (prevIsRiichiCreationCard)
                {
                    riichiableCards[riichiableCount++] = card;
                }
            }
            else
            {
                --globalOrders[card.GlobalOrder];

                var isTenpai = IsTenpai(agariContext, globalOrders);
                if (isTenpai)
                {
                    riichiableCards[riichiableCount++] = card;
                }

                ++globalOrders[card.GlobalOrder];

                prevGlobalOrder = card.GlobalOrder;
                prevIsRiichiCreationCard = isTenpai;
            }
        }

        return Fit_Card(riichiableCards, riichiableCount);
    }

    public void CheckTenpai(Card[] sealedCards, Card[] openedCards, AgariContext agariContext)
    {
        var sealedGlobalOrders = HandUtil.GetGlobalOrders(sealedCards);
        var openedGlobalOrders = HandUtil.GetGlobalOrders(openedCards);

        var globalOrders = HandUtil.SumGlobalOrders(sealedGlobalOrders, openedGlobalOrders);

        IsTenpai(agariContext, globalOrders);
    }

    bool IsTenpai(AgariContext agariContext, int[] globalOrders)
    {
        // 머리가 6개인가?
        if (Chiitoitsu.CheckTenpai(agariContext, globalOrders))
        {
            return true;
        }

        // 요구패가 13개인가?
        if (Kokushimusou.IsTenpai(globalOrders))
        {
            return true;
        }

        var ctxs = FindAll(globalOrders);

        foreach (object[] ctx in ctxs)
        {
            if (ctx == null) { continue; }

            var remainsGlobalOrders = Ctx.ReadGlobalOrders(ctx);
            var pairs = HandUtil.FindPairs(remainsGlobalOrders);

            foreach (var pair in pairs)
            {
                remainsGlobalOrders[pair] -= 2;
            }

            var bodies = Ctx.ReadChiCount(ctx) + Ctx.ReadPonCount(ctx);

            // 몸 4, 카드 1인 경우 -> 단면대기 텐파이            
            if (bodies == 4 && pairs.Length == 0)
            {
                agariContext.IsSingleWaiting = true;
                for (var i = 0; i < remainsGlobalOrders.Length; ++i)
                {
                    if (remainsGlobalOrders[i] > 0)
                    {
                        agariContext.AddAgariableGlobalOrder(i);

                        Debug.Log($"몸4 카드 1, 단면대기 텐파이 GlobalOrder:{i}");
                        break;
                    }
                }
            }
            // 몸 3, 머리 2인 경우 -> 양면대기 텐파이
            else if (bodies == 3 && pairs.Length == 2)
            {
                agariContext.IsSingleWaiting = false;
                agariContext.AddAgariableGlobalOrder(pairs[0]);
                agariContext.AddAgariableGlobalOrder(pairs[1]);

                Debug.Log($"몸4 머리 2, 양면대기 텐파이 GlobalOrder:{pairs[0]}, {pairs[1]}");
            }
            // 몸 3, 머리 1, 카드2개인 경우 -> 단면 or 양면 or 대기아님
            else if (bodies == 3 && pairs.Length == 1)
            {
                // 몸2 머리1에 34567인 경우는, 2 5 8로 삼면대기가 되는데 다음 경우로 분해가능
                // 1. 머리1, 몸2 + (345), (6, 7)남음 → 5, 8 양면대기
                // 2. 머리1, 몸2 + (567), (3, 4)남음 → 2, 5 양면대기
                // 따라서 남은 카드가 chiable한지 판단해보아야 함
                //  - 2, 4같이 한칸 떨어져 있다던지
                //  - 2, 3같이 붙어 있다던지

                for (var i = 0; i < 34 - 1; ++i)
                {
                    if (remainsGlobalOrders[i] == 1 && remainsGlobalOrders[i + 1] == 1)
                    {
                        agariContext.AddAgariableGlobalOrder(i);
                        agariContext.AddAgariableGlobalOrder(i + 1);
                        agariContext.IsSingleWaiting = false;

                        Debug.Log($"몸3 머리 1 카드 2, 양면대기 텐파이 GlobalOrder:{i}, {i+1}");
                        break;
                    }

                    if (i > 0 && remainsGlobalOrders[i - 1] == 1 && remainsGlobalOrders[i + 1] == 1)
                    {
                        agariContext.AddAgariableGlobalOrder(i);
                        agariContext.IsSingleWaiting = true;

                        Debug.Log($"몸3 머리 1 카드 2, 단면대기 텐파이 GlobalOrder:{i}");
                        break;
                    }
                }
            }
        }

        return agariContext.AgariableCount != 0;
    }

    object[] FindAll(int[] globalOrders)
    {
        if (globalOrders.Length != TILES_COUNT) Debug.Log("TILES LENGTH ERROR!");

        Result.Clear();

        var maxChiPonCount = 0;
        var localGlobalOrders = Clone(globalOrders);

        var manCtxs = Find(localGlobalOrders, HandUtil.GetManStartGlobalOrder(), HandUtil.GetManEndGlobalOrder(), true);
        var pinCtxs = Find(localGlobalOrders, HandUtil.GetPinStartGlobalOrder(), HandUtil.GetPinEndGlobalOrder(), true);
        var souCtxs = Find(localGlobalOrders, HandUtil.GetSouStartGlobalOrder(), HandUtil.GetSouEndGlobalOrder(), true);
        var wordCtxs = Find(localGlobalOrders, HandUtil.GetWordsStartGlobalOrder(), HandUtil.GetWordsEndGlobalOrder(), true);

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

        return Result.Clone();
    }

    // context[0]: int[38] remainTiles
    // context[1]: object[int[3]] chis
    // context[2]: int chiCount
    // context[3]: object[int[3]] pons
    // context[4]: int ponCount

    object[] Find(int[] originalGlobalOrders, int startOrder, int endOrder, bool startWithNull)
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

        if (startWithNull) { Result.Insert(0, null); }

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
        var manCtxs = Find(globalOrders, HandUtil.GetManStartGlobalOrder(), HandUtil.GetManEndGlobalOrder(), false);

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
        var manCtxs = Find(globalOrders, HandUtil.GetManStartGlobalOrder(), HandUtil.GetManEndGlobalOrder(), false);

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

        var resultCtxs = FindAll(globalOrders);

        // 1. (1,2,3) (1,2,3) (4,5,6) (8,8,8) (3, 7)
        // 2. (1,2,3) (1,2,3) (3,4,5) (8,8,8) (6, 7)
        // 3. (1,2,3) (1,2,3) (3,4,5) (6,7,8) (8, 8)
        // 4. (1,2,3) (1,2,3) (5,6,7) (8,8,8) (3, 4)
        DebugHelper.Equal(resultCtxs.Length, 4, 1);
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

        var chiable1 = (Card[])chiables[0];
        var chiable2 = (Card[])chiables[1];

        DebugHelper.IsTrue(chiable1[0].CardNumber == 1, 1);
        DebugHelper.IsTrue(chiable1[1].CardNumber == 3, 2);

        DebugHelper.IsTrue(chiable2[0].CardNumber == 3, 3);
        DebugHelper.IsTrue(chiable2[1].CardNumber == 4, 4);
    }

    void Test_Tenpai1()
    {
        DebugHelper.SetTestName("Test_Tenpai1");

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
                    TEST__SetTestData(TestComponents[10], "만", 6),
                    TEST__SetTestData(TestComponents[11], "만", 8),
                    TEST__SetTestData(TestComponents[12], "만", 8),
        };

        AgariContextForTest.Clear();

        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        DebugHelper.IsTrue(IsTenpai(AgariContextForTest, globalOrders), 1);
        DebugHelper.IsFalse(AgariContextForTest.IsSingleWaiting, 2);
        DebugHelper.Equal(AgariContextForTest.AgariableCardGlobalOrders[0], 5, 3);
        DebugHelper.Equal(AgariContextForTest.AgariableCardGlobalOrders[1], 7, 4);
    }

    void Test_Tenpai2()
    {
        DebugHelper.SetTestName("Test_Tenpai2");

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

                    TEST__SetTestData(TestComponents[9], "만", 4),
                    TEST__SetTestData(TestComponents[10], "만", 4),
                    TEST__SetTestData(TestComponents[11], "만", 4),

                    TEST__SetTestData(TestComponents[12], "만", 9),
        };

        AgariContextForTest.Clear();

        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        DebugHelper.IsTrue(IsTenpai(AgariContextForTest, globalOrders), 1);
        DebugHelper.IsTrue(AgariContextForTest.IsSingleWaiting, 2);
        DebugHelper.Equal(AgariContextForTest.AgariableCount, 1, 2);
        DebugHelper.Equal(AgariContextForTest.AgariableCardGlobalOrders[0], 8, 4);
    }

    public void Start()
    {
        if (DebugHelper != null && Networking.LocalPlayer == null && AgariContextForTest != null)
        {
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

            Test_Tenpai1();
            Test_Tenpai2();
        }
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

    public int[] Fit_Int(int[] arr, int count)
    {
        var newArr = new int[count];
        for (var i = 0; i < count; ++i)
        {
            newArr[i] = arr[i];
        }
        return newArr;
    }

    public Card[] Fit_Card(Card[] arr, int count)
    {
        var newArr = new Card[count];
        for (var i = 0; i < count; ++i)
        {
            newArr[i] = arr[i];
        }
        return newArr;
    }
}
