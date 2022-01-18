
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class HandCalculator : UdonSharpBehaviour
{
    [SerializeField] public DebugHelper DebugHelper;
    [SerializeField] public AgariContext AgariContextForTest;
    [SerializeField] public PlayerStatus PlayerStatusForTest;

    public Card[] TestComponents;

    const int TILES_COUNT = 34;

    [SerializeField] public KList Stack;
    [SerializeField] public KList Result;
    [SerializeField] public CalculatingContextHandler Ctx;
    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public ScoreCalculator ScoreCalculator;

    [SerializeField] public CalculateShanten CalculateShanten;

    [SerializeField] public Chiitoitsu Chiitoitsu;
    [SerializeField] public Kokushimusou Kokushimusou;
    [SerializeField] public NormalYaku NormalYaku;

    [SerializeField] public ResultViewer ResultViewer;

    // NOTE) 슌쯔, 커쯔를 영어로 쓰기가 귀찮고 길어서 Chi, Pon으로 줄여서 씀

    bool IsSpecialYaku(int[] globalOrders, PlayerStatus playerStatus)
    {
        var pairs = HandUtil.FindPairs(globalOrders);

        // 특수역: 국싸무쌍
        var count = HandUtil.GetYaojuhaiTypeCount(globalOrders);
        if (count == 13)
        {
            playerStatus.AddHan("Kokushimusou", 9999); // 역만은 판이 어떻게 되지..?
            playerStatus.Fu = 999; // 역만은 부수가 어떻게 되지...?
            return true;
        }

        return false;
    }

    public void RequestTsumoScore(Card[] sealedCards, Card[] openedCards, AgariContext agariContext, PlayerStatus playerStatus)
    {
        var sealedGlobalOrders = HandUtil.GetGlobalOrders(sealedCards);
        var openedGlobalOrders = HandUtil.GetGlobalOrders(openedCards);
        var globalOrders = HandUtil.SumGlobalOrders(sealedGlobalOrders, openedGlobalOrders);

        if (IsSpecialYaku(globalOrders, playerStatus))
        {
            return;
        }

        var ctxs = FindAll(globalOrders);
        ScoreCalculator.CalculateTsumo(playerStatus, agariContext, sealedCards, openedCards, ctxs);
    }

    public void RequestRonScore(Card[] sealedCards, Card[] openedCards, AgariContext agariContext, PlayerStatus playerStatus)
    {
        var sealedGlobalOrders = HandUtil.GetGlobalOrders(sealedCards);
        var openedGlobalOrders = HandUtil.GetGlobalOrders(openedCards);
        var globalOrders = HandUtil.SumGlobalOrders(sealedGlobalOrders, openedGlobalOrders);

        if (IsSpecialYaku(globalOrders, playerStatus))
        {
            return;
        }

        var ctxs = FindAll(globalOrders);
        ScoreCalculator.CalculateRon(playerStatus, agariContext, sealedCards, openedCards, ctxs);
    }

    public void RequestNakiable(Card[] cards, PlayerStatus playerStatus, UIContext uiContext, AgariContext agariContext, Card discardedCard, bool isDiscardedByLeftPlayer)
    {
        var chiableList = GetChiableAll(cards, discardedCard);
        var isChiable = chiableList.Length > 0 && isDiscardedByLeftPlayer;

        if (!playerStatus.IsRiichiMode)
        {
            if (!playerStatus.isNoNakiMode)
            {
                uiContext.IsChiable = isChiable;
                uiContext.IsPonable = IsPonable(cards, discardedCard);
                uiContext.IsKkanable = IsKkanable(cards, discardedCard);
            }

            if (isChiable)
            {
                SetChiableOnUiContext(uiContext, chiableList);
            }
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

        for (var index = 0; index < globalOrders.Length; index++)
        {
            if (globalOrders[index] == 4)
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

                var ctxs = FindAll(globalOrders);

                var isTenpai = IsTenpai(agariContext, ctxs, globalOrders);
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

    public void CheckTenpai(Card[] sealedCards, Card[] openedCards, AgariContext agariContext, UIContext uIContext)
    {
        {
            var str = "[HandCalculator]\nSealed : ";
            foreach (Card card in sealedCards)
            {
                str += $"{card.ToString()}, ";
            }
            str += "\nOpenned : ";
            foreach (Card card in openedCards)
            {
                str += $"{card.ToString()}, ";
            }
            Debug.Log(str);
        }

        var sealedGlobalOrders = HandUtil.GetGlobalOrders(sealedCards);
        var openedGlobalOrders = HandUtil.GetGlobalOrders(openedCards);

        var globalOrders = HandUtil.SumGlobalOrders(sealedGlobalOrders, openedGlobalOrders);

        var ctxs = FindAll(globalOrders);

        IsTenpai(agariContext, ctxs, globalOrders);

        //agariContext.SetShantenCount(CheckShanten(ctxs)); //샹텐계산은 일단 기능 제외

        //uIContext.SetAgarible(agariContext.AgariableCardGlobalOrders);
    }

    bool IsTenpai(AgariContext agariContext, object[] ctxs, int[] globalOrders)
    {
        // 머리가 6개인가?
        if (Chiitoitsu.CheckTenpai(agariContext, globalOrders))
        {
            return true;
        }

        // 요구패가 13개인가?
        if (Kokushimusou.CheckTenpai(agariContext, globalOrders))
        {
            return true;
        }

        if (NormalYaku.CheckTenpai(Ctx, ctxs, agariContext, globalOrders))
        {
            return true;
        }

        return false;
    }

    int CheckShanten(object[] ctxs)
    {
        var min = 8;

        foreach(object[] ctx in ctxs)
        {
            var shanten = NormalYaku.CheckShantenFromTenpai(Ctx, ctx);

            if(shanten < min)
            {
                min = shanten;
            }

            Debug.Log($"[HandCalculator] shanten {shanten}");
        }

        return min * -1;
    }

    object[] FindAll(int[] globalOrders)
    {
        if (globalOrders.Length != TILES_COUNT) Debug.Log("TILES LENGTH ERROR!");

        Result.Clear();

        var maxChiPonCount = 0;
        var localGlobalOrders = Clone(globalOrders);

        var manCtxs = Find(localGlobalOrders, HandUtil.GetManStartGlobalOrder(), HandUtil.GetManEndGlobalOrder());
        var pinCtxs = Find(localGlobalOrders, HandUtil.GetPinStartGlobalOrder(), HandUtil.GetPinEndGlobalOrder());
        var souCtxs = Find(localGlobalOrders, HandUtil.GetSouStartGlobalOrder(), HandUtil.GetSouEndGlobalOrder());
        var wordCtxs = Find(localGlobalOrders, HandUtil.GetWordsStartGlobalOrder(), HandUtil.GetWordsEndGlobalOrder());

        /*if (manCtxs[0] == null)
            Debug.Log("[HandCalculator] manCtxs 이게 대체 뭔데 NULL이야!!!");
        if (pinCtxs[0] == null)
            Debug.Log("[HandCalculator] pinCtxs 이게 대체 뭔데 NULL이야!!!");
        if (souCtxs[0] == null)
            Debug.Log("[HandCalculator] souCtxs 이게 대체 뭔데 NULL이야!!!");
        if (wordCtxs[0] == null)
            Debug.Log("[HandCalculator] wordCtxs 이게 대체 뭔데 NULL이야!!!");*/

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

                        var newGlobalOrders = Clone(globalOrders);
                        var globalOrdersFromChiPon = Ctx.GetGlobalOrdersFromChiPon(ctx);
                        for (var i = 0; i < 34; ++i)
                        {
                            newGlobalOrders[i] -= globalOrdersFromChiPon[i];
                        }

                        Ctx.ApplyGlobalOrders(ctx, newGlobalOrders);

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

        if (Result.Count() == 0)
        {
            Result.Insert(0, first);
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

        var resultCtxs = FindAll(globalOrders);

        // 1. (1,2,3) (1,2,3) (4,5,6) (8,8,8) (3, 7)
        // 2. (1,2,3) (1,2,3) (3,4,5) (8,8,8) (6, 7)
        // 3. (1,2,3) (2,3,4) (5,6,7) (8,8,8) (1, 3)
        // 4. (1,2,3) (1,2,3) (3,4,5) (6,7,8) (8, 8)
        // 5. (1,2,3) (1,2,3) (5,6,7) (8,8,8) (3, 4)
        DebugHelper.Equal(resultCtxs.Length, 5, 1);
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

        AgariContextForTest.Clear();

        DebugHelper.IsTrue(Chiitoitsu.CheckTenpai(AgariContextForTest, globalOrders), 1);

        TEST__SetTestData(TestComponents[13], "만", 7);

        DebugHelper.IsTrue(AgariContextForTest.IsAgariable(TestComponents[13]), 2);
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

        AgariContextForTest.Clear();

        DebugHelper.IsTrue(Kokushimusou.CheckTenpai(AgariContextForTest, globalOrders), 1);

        TEST__SetTestData(TestComponents[13], "중", 7);

        DebugHelper.IsTrue(AgariContextForTest.IsAgariable(TestComponents[13]), 2);
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

        //DebugHelper.IsTrue(IsTenpai(AgariContextForTest, globalOrders), 1);
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

        //DebugHelper.IsTrue(IsTenpai(AgariContextForTest, globalOrders), 1);
        DebugHelper.IsTrue(AgariContextForTest.IsSingleWaiting, 2);
        DebugHelper.Equal(AgariContextForTest.AgariableCount, 1, 2);
        DebugHelper.Equal(AgariContextForTest.AgariableCardGlobalOrders[0], 8, 4);
    }

    void Test_Tenpai3()
    {
        DebugHelper.SetTestName("Test_Tenpai3");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 5),
                    TEST__SetTestData(TestComponents[1], "만", 6),
                    TEST__SetTestData(TestComponents[2], "만", 7),

                    TEST__SetTestData(TestComponents[3], "만", 7),
                    TEST__SetTestData(TestComponents[4], "만", 8),
                    TEST__SetTestData(TestComponents[5], "만", 9),

                    TEST__SetTestData(TestComponents[6], "통", 2),
                    TEST__SetTestData(TestComponents[7], "통", 3),
                    TEST__SetTestData(TestComponents[8], "통", 4),

                    TEST__SetTestData(TestComponents[9], "통", 9),

                    TEST__SetTestData(TestComponents[10], "삭", 7),
                    TEST__SetTestData(TestComponents[11], "삭", 8),
                    TEST__SetTestData(TestComponents[12], "삭", 9),
        };

        AgariContextForTest.Clear();

        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        //DebugHelper.IsTrue(IsTenpai(AgariContextForTest, globalOrders), 1);
        DebugHelper.IsTrue(AgariContextForTest.IsSingleWaiting, 2);
        DebugHelper.Equal(AgariContextForTest.AgariableCardGlobalOrders[0], 17, 3);

        TEST__SetTestData(TestComponents[13], "통", 9);

        DebugHelper.IsTrue(AgariContextForTest.IsAgariable(TestComponents[13]), 4);
    }

    void Test_Tenpai4()
    {
        DebugHelper.SetTestName("Test_Tenpai4");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 2),
                    TEST__SetTestData(TestComponents[1], "만", 3),
                    TEST__SetTestData(TestComponents[2], "만", 4),

                    TEST__SetTestData(TestComponents[3], "남", 2),
                    TEST__SetTestData(TestComponents[4], "통", 2),
                    TEST__SetTestData(TestComponents[5], "통", 3),

                    TEST__SetTestData(TestComponents[6], "통", 4)
        };

        var testOpennedSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[7], "백", 5),
                    TEST__SetTestData(TestComponents[9], "백", 5),
                    TEST__SetTestData(TestComponents[10], "서", 3),
                    TEST__SetTestData(TestComponents[12], "서", 3)
        };

        AgariContextForTest.Clear();

        var sealedGlobalOrders = HandUtil.GetGlobalOrders(testSet);
        var openedGlobalOrders = HandUtil.GetGlobalOrders(testOpennedSet);

        var globalOrders = HandUtil.SumGlobalOrders(sealedGlobalOrders, openedGlobalOrders);

        //DebugHelper.IsTrue(IsTenpai(AgariContextForTest, globalOrders), 1);
        DebugHelper.IsTrue(AgariContextForTest.IsSingleWaiting, 2);
        DebugHelper.Equal(AgariContextForTest.AgariableCardGlobalOrders[0], 3, 3);
    }

    void Test_Tsumo1()
    {
        // 여기는 함수랑 이름 똑같게
        DebugHelper.SetTestName("Test_Tsumo1");

        // 텐파이 상태로 만들고 시작합니다
        // 왜냐하면 패가 하나씩 오기 때문에 무조건 텐파이 상태를 거쳐서
        // 텐파이 여부를 판단한 후 그걸 바탕으로 화료를 결정하기 때문
        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 5),
                    TEST__SetTestData(TestComponents[1], "만", 6),
                    TEST__SetTestData(TestComponents[2], "만", 7),

                    TEST__SetTestData(TestComponents[3], "통", 4),
                    TEST__SetTestData(TestComponents[4], "통", 5),
                    TEST__SetTestData(TestComponents[5], "통", 6),

                    TEST__SetTestData(TestComponents[6], "삭", 2),
                    TEST__SetTestData(TestComponents[7], "삭", 2),
                    TEST__SetTestData(TestComponents[8], "삭", 2),

                    TEST__SetTestData(TestComponents[9], "삭", 6),
                    TEST__SetTestData(TestComponents[10], "삭", 7),
                    TEST__SetTestData(TestComponents[11], "삭", 8),

                    TEST__SetTestData(TestComponents[12], "통", 3),
        };


        // 글로벌오더로 전환
        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        // 쓰기 전에 Clear
        AgariContextForTest.Clear();
        // IsTenpai를 부르는 순간 AgariContext에 값이 할당됨
        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        // isTenpai값이 아니면 아래와 같은 메세지가 뜹니다
        // "Test_Tsumo1의 1번 라인이 참이어야 하는데 참이 아닙니다"
        // 내부 구현은 한번 보면 이해하실듯
        //DebugHelper.IsTrue(isTenpai, 1); // 맨뒤에 1은 라인번호

        // 새로받은 카드 할당
        TEST__SetTestData(TestComponents[13], "통", 3);

        // 화료 가능한지 체크
        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);
        // 화료 맞는지 디버깅 라인 체크
        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length"+TestComponents.Length);

        PlayerStatusForTest.Initialize(); // 쓰기 전 초기화
        PlayerStatusForTest.IsRiichiMode = true; // 테스트로 리치라고 쳐줌
        PlayerStatusForTest.IsOneShotRiichi = false; // 일발리치 아님
        PlayerStatusForTest.IsFirstOrder= false; // 더블리치는 아님
        // 쯔모 계산 요청
        RequestTsumoScore(TestComponents, new Card[] { }, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount; // 총 역 갯수
        var yaku = PlayerStatusForTest.YakuKey; // string[] 배열 역 key가 들어있음
        var han = PlayerStatusForTest.Han; // int[] 역마다 판 적혀있음
        var fu = PlayerStatusForTest.Fu; // 총 부수

        for(var i=0; i<count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        // 이런 식으로 맞는 결과를 써줌
        DebugHelper.IsTrue(count == 2, 3); // 역 1개
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4); // 리치 
        DebugHelper.IsTrue(han[0] == 1, 5); // 1판
        DebugHelper.IsTrue(yaku[1] == "AllSimples", 6); // 탕야오
        DebugHelper.IsTrue(han[1] == 1, 7); // 1판
    }


    void Test_Tsumo2()
    {
        DebugHelper.SetTestName("Test_Tsumo2");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "삭", 2),
                    TEST__SetTestData(TestComponents[1], "삭", 2),
                    TEST__SetTestData(TestComponents[2], "삭", 2),

                    TEST__SetTestData(TestComponents[3], "삭", 2),
                    TEST__SetTestData(TestComponents[4], "삭", 3),
                    TEST__SetTestData(TestComponents[5], "삭", 4),

                    TEST__SetTestData(TestComponents[6], "삭", 6),
                    TEST__SetTestData(TestComponents[7], "삭", 6),
                    TEST__SetTestData(TestComponents[8], "삭", 6),

                    TEST__SetTestData(TestComponents[9], "삭", 8),
                    TEST__SetTestData(TestComponents[10], "삭", 8),
                    TEST__SetTestData(TestComponents[11], "삭", 8),

                    TEST__SetTestData(TestComponents[12], "발", 6),
        };
        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        AgariContextForTest.Clear();
        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        //DebugHelper.IsTrue(isTenpai, 1);

        TEST__SetTestData(TestComponents[13], "발", 6);

        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);
        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length" + TestComponents.Length);

        PlayerStatusForTest.Initialize();
        PlayerStatusForTest.IsRiichiMode = true;
        PlayerStatusForTest.IsOneShotRiichi = false;
        PlayerStatusForTest.IsFirstOrder = false;

        RequestTsumoScore(TestComponents, new Card[] { }, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount;
        var yaku = PlayerStatusForTest.YakuKey;
        var han = PlayerStatusForTest.Han;
        var fu = PlayerStatusForTest.Fu;

        for (var i = 0; i < count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        DebugHelper.IsTrue(count == 2, 3);
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4);
        DebugHelper.IsTrue(han[0] == 1, 5);
        DebugHelper.IsTrue(yaku[1] == "AllGreen", 6);
        DebugHelper.IsTrue(han[1] == 13, 7);
    }

    void Test_Tsumo3()
    {
        DebugHelper.SetTestName("Test_Tsumo3");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "삭", 2),
                    TEST__SetTestData(TestComponents[1], "삭", 2),

                    TEST__SetTestData(TestComponents[2], "삭", 1),
                    TEST__SetTestData(TestComponents[3], "삭", 1),
                    TEST__SetTestData(TestComponents[4], "삭", 1),

                    TEST__SetTestData(TestComponents[5], "삭", 5),
                    TEST__SetTestData(TestComponents[6], "삭", 6),

                    TEST__SetTestData(TestComponents[7], "삭", 4),
                    TEST__SetTestData(TestComponents[8], "삭", 4),
                    TEST__SetTestData(TestComponents[9], "삭", 4),

                    TEST__SetTestData(TestComponents[10], "삭", 7),
                    TEST__SetTestData(TestComponents[11], "삭", 8),
                    TEST__SetTestData(TestComponents[12], "삭", 9),
        };

        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        AgariContextForTest.Clear();
        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        //DebugHelper.IsTrue(isTenpai, 1);

        TEST__SetTestData(TestComponents[13], "삭", 4);

        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);
        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length" + TestComponents.Length);

        PlayerStatusForTest.Initialize();
        PlayerStatusForTest.IsRiichiMode = true; 
        PlayerStatusForTest.IsOneShotRiichi = false; 
        PlayerStatusForTest.IsFirstOrder = false; 
        // 쯔모 계산 요청
        RequestTsumoScore(TestComponents, new Card[] { }, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount;
        var yaku = PlayerStatusForTest.YakuKey;
        var han = PlayerStatusForTest.Han;
        var fu = PlayerStatusForTest.Fu;

        ResultViewer.setResult("테스트 쯔모", "System", count, yaku, han, fu, 0);

        for (var i = 0; i < count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        DebugHelper.IsTrue(count == 2, 3);
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4); 
        DebugHelper.IsTrue(han[0] == 1, 5);
        DebugHelper.IsTrue(yaku[1] == "ClearFlush", 6);
        DebugHelper.IsTrue(han[1] == 6, 7);
    }

    void Test_Tsumo4()
    {
        DebugHelper.SetTestName("Test_Tsumo4");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "동", 1),
                    TEST__SetTestData(TestComponents[1], "동", 1),
                    TEST__SetTestData(TestComponents[2], "동", 1),
                    
                    TEST__SetTestData(TestComponents[3], "만", 1),
                    TEST__SetTestData(TestComponents[4], "만", 2),
                    TEST__SetTestData(TestComponents[5], "만", 3),

                    TEST__SetTestData(TestComponents[6], "만", 5),
                    TEST__SetTestData(TestComponents[7], "만", 5),

                    TEST__SetTestData(TestComponents[8], "만", 7),
                    TEST__SetTestData(TestComponents[9], "만", 8),

                    TEST__SetTestData(TestComponents[10], "북", 4),
                    TEST__SetTestData(TestComponents[11], "북", 4),
                    TEST__SetTestData(TestComponents[12], "북", 4),
        };

        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        AgariContextForTest.Clear();

        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        //DebugHelper.IsTrue(isTenpai, 1);

        TEST__SetTestData(TestComponents[13], "만", 6);

        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);

        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length" + TestComponents.Length);

        PlayerStatusForTest.Initialize();
        PlayerStatusForTest.IsRiichiMode = true;
        PlayerStatusForTest.IsOneShotRiichi = false;
        PlayerStatusForTest.IsFirstOrder = false;

        RequestTsumoScore(TestComponents, new Card[] { }, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount;
        var yaku = PlayerStatusForTest.YakuKey;
        var han = PlayerStatusForTest.Han; 
        var fu = PlayerStatusForTest.Fu;

        for (var i = 0; i < count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        DebugHelper.IsTrue(count == 2, 3);
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4);
        DebugHelper.IsTrue(han[0] == 1, 5); 
        DebugHelper.IsTrue(yaku[1] == "HalfFlush", 6);
        DebugHelper.IsTrue(han[1] == 6, 7);
    }

    void Test_Tsumo5()
    {
        DebugHelper.SetTestName("Test_Tsumo5");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1),
                    TEST__SetTestData(TestComponents[1], "만", 1),
                    TEST__SetTestData(TestComponents[2], "만", 1),

                    TEST__SetTestData(TestComponents[3], "만", 2),
                    TEST__SetTestData(TestComponents[4], "만", 3),
                    TEST__SetTestData(TestComponents[5], "만", 4),

                    TEST__SetTestData(TestComponents[6], "만", 5),
                    TEST__SetTestData(TestComponents[7], "만", 6),

                    TEST__SetTestData(TestComponents[8], "만", 7),
                    TEST__SetTestData(TestComponents[9], "만", 8),

                    TEST__SetTestData(TestComponents[10], "만", 9),
                    TEST__SetTestData(TestComponents[11], "만", 9),
                    TEST__SetTestData(TestComponents[12], "만", 9),
        };


        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        AgariContextForTest.Clear();
        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        //DebugHelper.IsTrue(isTenpai, 1);

        TEST__SetTestData(TestComponents[13], "만", 8);

        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);
        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length" + TestComponents.Length);

        PlayerStatusForTest.Initialize(); 
        PlayerStatusForTest.IsRiichiMode = true; 
        PlayerStatusForTest.IsOneShotRiichi = false; 
        PlayerStatusForTest.IsMenzen = true; 
        PlayerStatusForTest.IsFirstOrder = false; 

        RequestTsumoScore(TestComponents, new Card[] { }, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount;
        var yaku = PlayerStatusForTest.YakuKey;
        var han = PlayerStatusForTest.Han;
        var fu = PlayerStatusForTest.Fu;

        for (var i = 0; i < count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        DebugHelper.IsTrue(count == 2, 3); 
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4); 
        DebugHelper.IsTrue(han[0] == 1, 5);
        DebugHelper.IsTrue(yaku[1] == "NineGates", 6);
        DebugHelper.IsTrue(han[1] == 13, 7);
    }
    void Test_Tsumo6()
    {
        DebugHelper.SetTestName("Test_Tsumo6");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "동", 1),
                    TEST__SetTestData(TestComponents[1], "동", 1),

                    TEST__SetTestData(TestComponents[2], "남", 2),
                    TEST__SetTestData(TestComponents[3], "남", 2),

                    TEST__SetTestData(TestComponents[4], "서", 3),
                    TEST__SetTestData(TestComponents[5], "서", 3),

                    TEST__SetTestData(TestComponents[6], "북", 4),
                    TEST__SetTestData(TestComponents[7], "북", 4),

                    TEST__SetTestData(TestComponents[8], "백", 5),
                    TEST__SetTestData(TestComponents[9], "백", 5),

                    TEST__SetTestData(TestComponents[10], "발", 6),
                    TEST__SetTestData(TestComponents[11], "발", 6),

                    TEST__SetTestData(TestComponents[12], "중", 7),
        };


        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        AgariContextForTest.Clear();
        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        //DebugHelper.IsTrue(isTenpai, 1); 

        TEST__SetTestData(TestComponents[13], "중", 7);

        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);
        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length" + TestComponents.Length);

        PlayerStatusForTest.Initialize(); 
        PlayerStatusForTest.IsRiichiMode = true; 
        PlayerStatusForTest.IsOneShotRiichi = false; 
        PlayerStatusForTest.IsFirstOrder = false; 

        RequestTsumoScore(TestComponents, new Card[] { }, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount; 
        var yaku = PlayerStatusForTest.YakuKey;
        var han = PlayerStatusForTest.Han; 
        var fu = PlayerStatusForTest.Fu;

        for (var i = 0; i < count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        DebugHelper.IsTrue(count == 2, 3); 
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4);
        DebugHelper.IsTrue(han[0] == 1, 5);
        DebugHelper.IsTrue(yaku[1] == "NineGates", 6); 
        DebugHelper.IsTrue(han[1] == 13, 7);
    }

    void Test_Tsumo7()
    {
        DebugHelper.SetTestName("Test_Tsumo7");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1),
                    TEST__SetTestData(TestComponents[1], "만", 1),
                    TEST__SetTestData(TestComponents[2], "만", 1),

                    TEST__SetTestData(TestComponents[3], "만", 9),
                    TEST__SetTestData(TestComponents[4], "만", 9),
                    TEST__SetTestData(TestComponents[5], "만", 9),

                    TEST__SetTestData(TestComponents[6], "삭", 1),
                    TEST__SetTestData(TestComponents[7], "삭", 1),
                    TEST__SetTestData(TestComponents[8], "삭", 1),

                    TEST__SetTestData(TestComponents[9], "삭", 9),
                    TEST__SetTestData(TestComponents[10], "삭", 9),

                    TEST__SetTestData(TestComponents[11], "통", 1),
                    TEST__SetTestData(TestComponents[12], "통", 1),
        };


        var globalOrders = HandUtil.GetGlobalOrders(testSet);

        AgariContextForTest.Clear();
        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        //DebugHelper.IsTrue(isTenpai, 1);

        TEST__SetTestData(TestComponents[13], "삭", 9);

        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);
        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length" + TestComponents.Length);

        PlayerStatusForTest.Initialize();
        PlayerStatusForTest.IsRiichiMode = true;
        PlayerStatusForTest.IsOneShotRiichi = false;
        PlayerStatusForTest.IsFirstOrder = false;

        RequestTsumoScore(TestComponents, new Card[] { }, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount;
        var yaku = PlayerStatusForTest.YakuKey;
        var han = PlayerStatusForTest.Han;
        var fu = PlayerStatusForTest.Fu;

        for (var i = 0; i < count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        DebugHelper.IsTrue(count == 2, 3);
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4);
        DebugHelper.IsTrue(han[0] == 1, 5);
        DebugHelper.IsTrue(yaku[1] == "AllTerminals", 6);
        DebugHelper.IsTrue(han[1] == 13, 7);
    }

    void Test_Tsumo8()
    {
        DebugHelper.SetTestName("Test_Tsumo8");

        var testSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 2),
                    TEST__SetTestData(TestComponents[1], "만", 3),
                    TEST__SetTestData(TestComponents[2], "만", 4),

                    TEST__SetTestData(TestComponents[3], "남", 2),
                    TEST__SetTestData(TestComponents[4], "통", 2),
                    TEST__SetTestData(TestComponents[5], "통", 3),

                    TEST__SetTestData(TestComponents[6], "통", 4)
        };

        var testOpennedSet = new Card[]
        {
                    TEST__SetTestData(TestComponents[7], "백", 5),
                    TEST__SetTestData(TestComponents[8], "백", 5),

                    TEST__SetTestData(TestComponents[9], "백", 5),
                    TEST__SetTestData(TestComponents[10], "서", 3),

                    TEST__SetTestData(TestComponents[11], "서", 3),
                    TEST__SetTestData(TestComponents[12], "서", 3),
        };

        var sealedGlobalOrders = HandUtil.GetGlobalOrders(testSet);
        var openedGlobalOrders = HandUtil.GetGlobalOrders(testOpennedSet);

        var globalOrders = HandUtil.SumGlobalOrders(sealedGlobalOrders, openedGlobalOrders);

        AgariContextForTest.Clear();
        //var isTenpai = IsTenpai(AgariContextForTest, globalOrders);

        //DebugHelper.IsTrue(isTenpai, 1);

        TEST__SetTestData(TestComponents[13], "남", 2);

        var isAgariable = AgariContextForTest.IsAgariable(TestComponents[13]);
        DebugHelper.IsTrue(isAgariable, 2);

        Debug.Log("length" + TestComponents.Length);

        PlayerStatusForTest.Initialize();
        PlayerStatusForTest.IsRiichiMode = true;
        PlayerStatusForTest.IsOneShotRiichi = false;
        PlayerStatusForTest.IsFirstOrder = false;

        RequestTsumoScore(testSet, testOpennedSet, AgariContextForTest, PlayerStatusForTest);

        var count = PlayerStatusForTest.YakuCount;
        var yaku = PlayerStatusForTest.YakuKey;
        var han = PlayerStatusForTest.Han;
        var fu = PlayerStatusForTest.Fu;

        for (var i = 0; i < count; ++i)
        {
            Debug.Log($"{yaku[i]} {han[i]}");
        }

        DebugHelper.IsTrue(count == 2, 3);
        DebugHelper.IsTrue(yaku[0] == "Riichi", 4);
        DebugHelper.IsTrue(han[0] == 1, 5);
        DebugHelper.IsTrue(yaku[1] == "AllTerminals", 6);
        DebugHelper.IsTrue(han[1] == 13, 7);
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
            Test_Tenpai3();
            Test_Tenpai4();

            Test_Tsumo1();
            Test_Tsumo2();
            Test_Tsumo3();
            Test_Tsumo4();
            Test_Tsumo5();
            Test_Tsumo6();
            Test_Tsumo7();
            Test_Tsumo8();
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
