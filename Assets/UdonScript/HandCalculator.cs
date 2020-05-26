
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandCalculator : UdonSharpBehaviour
{
    public bool IgnoreTests = false;
    public CardComponent[] TestComponents;

    const int TILES_COUNT = 34;

    public KList Stack;
    public KList Result;
    public ContextHandler Ctx;

    public HandUtil HandUtil;
    public Chiitoitsu Chiitoitsu;
    public Kokushimusou Kokushimusou;

    // NOTE) 슌쯔, 커쯔를 영어로 쓰기가 귀찮고 길어서 Chi, Pon으로 줄여서 씀

    public bool IsChiable(CardComponent[] cards, CardComponent discardedCard)
    {
        var tiles = HandUtil.CardComponetsToIndexes(cards);
        var chiIndex = HandUtil.CardComponentToIndex(discardedCard);

        // 자패의 슌쯔는 검사하지 않는다
        if (chiIndex >= HandUtil.GetWordsStartIndex()) { return false; }

        // 같은 type의 패만 슌쯔를 검사한다
        var typeStartIndex = HandUtil.GetStartIndexOfType(discardedCard.Type);
        var typeEndIndex = HandUtil.GetEndIndexOfType(discardedCard.Type);

        if (typeStartIndex + 2 <= chiIndex && chiIndex <= typeEndIndex - 0 && tiles[chiIndex - 2] > 0 && tiles[chiIndex - 1] > 0) return true;
        if (typeStartIndex + 1 <= chiIndex && chiIndex <= typeEndIndex - 1 && tiles[chiIndex - 1] > 0 && tiles[chiIndex + 1] > 0) return true;
        if (typeStartIndex + 0 <= chiIndex && chiIndex <= typeEndIndex - 2 && tiles[chiIndex + 1] > 0 && tiles[chiIndex + 2] > 0) return true;

        return false;
    }

    public bool IsPonable(CardComponent[] cards, CardComponent discardedCard)
    {
        var tiles = HandUtil.CardComponetsToIndexes(cards);
        var ponIndex = HandUtil.CardComponentToIndex(discardedCard);

        return tiles[ponIndex] + 1 >= 3;
    }

    public bool IsKkanable(CardComponent[] cards, CardComponent discardedCard)
    {
        var tiles = HandUtil.CardComponetsToIndexes(cards);
        var ponIndex = HandUtil.CardComponentToIndex(discardedCard);

        return tiles[ponIndex] + 1 == 4;
    }

    public int[] IsRiichiable(CardComponent[] cards)
    {
        if (cards.Length != 14) Debug.Log("cards.Length != 14");

        // 버려서 텐파이 되는 곳을 찾는다
        var riichiCreationIndex = new int[TILES_COUNT];

        var tiles = HandUtil.CardComponetsToIndexes(cards);
        for (var i = 0; i < tiles.Length; ++i)
        {
            if (tiles[i] > 0)
            {
                --tiles[i];
                if (IsTenpai(tiles))
                {
                    riichiCreationIndex[i] = 1;
                }
                ++tiles[i];
            }
        }

        return riichiCreationIndex;
    }

    public bool IsTenpai(int[] tiles)
    {
        // 머리가 6개인가?
        if (Chiitoitsu.IsTenpai(tiles))
        {
            return true;
        }

        // 요구패가 13개인가?
        if (Kokushimusou.IsTenpai(tiles))
        {
            return true;
        }

        // check normal
        return true;
    }

    object[] FindAll(int[] tiles, int[] openedTiles)
    {
        if (tiles.Length != TILES_COUNT) Debug.Log("TILES LENGTH ERROR!");

        Result.Clear();

        var maxChiPonCount = 0;
        foreach (var pairIndex in HandUtil.FindPairs(tiles))
        {
            var localTiles = Clone(tiles);
            localTiles[pairIndex] -= 2;

            var manCtxs = Find(localTiles, HandUtil.GetManStartIndex(), HandUtil.GetManEndIndex());
            var pinCtxs = Find(localTiles, HandUtil.GetPinStartIndex(), HandUtil.GetPinEndIndex());
            var souCtxs = Find(localTiles, HandUtil.GetSouStartIndex(), HandUtil.GetSouEndIndex());
            var wordCtxs = Find(localTiles, HandUtil.GetWordsStartIndex(), HandUtil.GetWordsEndIndex());

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
                                var t = Ctx.ReadTiles(ctx);
                                t[pairIndex] += 2;

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

    object[] Find(int[] originalTiles, int startIndex, int endIndex)
    {
        // 한개도 안 되면 리턴하지 말기
        var maxChiPonCount = 1;
        var first = Ctx.CreateContext(originalTiles);

        Stack.Clear();
        Result.Clear();

        Stack.Add(first);

        while (Stack.Count() != 0)
        {
            var top = (object[])Stack.RemoveLast();
            var tiles = Ctx.ReadTiles(top);
            var chiCount = Ctx.ReadChiCount(top);
            var ponCount = Ctx.ReadPonCount(top);
            var isChanged = false;

            // 자패의 슌쯔는 검사하지 않는다
            if (startIndex < HandUtil.GetWordsStartIndex())
            {
                for (var i = startIndex; i <= endIndex - 2; ++i)
                {
                    // 치는 뭐부터 먼저 하는지에 따라 순서가 갈려서 각 경우 전부 검사해야 함 
                    // 11234의 경우, (123)(14)와 (11)(234)가 가능하다
                    if (CanChi(tiles, i))
                    {
                        var context = Ctx.CopyContext(top);
                        Ctx.ApplyChi(context, i);

                        if (!Ctx.HasSameTiles(Stack.Clone(), context))
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

            for (var i = startIndex; i <= endIndex; ++i)
            {
                if (CanPon(tiles, i))
                {
                    var context = Ctx.CopyContext(top);
                    Ctx.ApplyPon(context, i);

                    if (!Ctx.HasSameTiles(Stack.Clone(), context))
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

    bool CanPon(int[] tiles, int startIndex)
    {
        return tiles[startIndex] > 2;
    }

    bool CanChi(int[] tiles, int startIndex)
    {
        if (startIndex + 2 >= tiles.Length) { return false; }
        return (tiles[startIndex] > 0 && tiles[startIndex + 1] > 0 && tiles[startIndex + 2] > 0);
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
        var testSet = new CardComponent[]
        {
            TEST__SetTestData(TestComponents[0], "만", 1, 0),
            TEST__SetTestData(TestComponents[1], "만", 1, 1),
            TEST__SetTestData(TestComponents[2], "만", 2, 2),
            TEST__SetTestData(TestComponents[3], "만", 2, 3),
            TEST__SetTestData(TestComponents[4], "만", 3, 4),
            TEST__SetTestData(TestComponents[5], "만", 3, 5)
        };

        var tiles = HandUtil.CardComponetsToIndexes(testSet);
        var manCtxs = Find(tiles, HandUtil.GetManStartIndex(), HandUtil.GetManEndIndex());

        if (manCtxs.Length != 1) Debug.Log("error1");

        // (1,2,3) (1,2,3)
        if (Ctx.TEST__GetChiCount(manCtxs, 0, 0) != 2) Debug.Log("error2");
    }

    void Test2()
    {
        var testSet = new CardComponent[]
          {
                    TEST__SetTestData(TestComponents[0], "만", 1, 0),
                    TEST__SetTestData(TestComponents[1], "만", 1, 1),
                    TEST__SetTestData(TestComponents[2], "만", 1, 2),
                    TEST__SetTestData(TestComponents[3], "만", 2, 3),
                    TEST__SetTestData(TestComponents[4], "만", 2, 4),
                    TEST__SetTestData(TestComponents[5], "만", 2, 5),
                    TEST__SetTestData(TestComponents[6], "만", 3, 6),
                    TEST__SetTestData(TestComponents[7], "만", 3, 7),
                    TEST__SetTestData(TestComponents[8], "만", 3, 8),
                    TEST__SetTestData(TestComponents[9], "만", 9, 9)
      };

        var tiles = HandUtil.CardComponetsToIndexes(testSet);
        var manCtxs = Find(tiles, HandUtil.GetManStartIndex(), HandUtil.GetManEndIndex());

        if (manCtxs.Length != 2) Debug.Log("error");

        // (1,1,1) (2,2,2) (3,3,3)
        if (Ctx.TEST__GetPonCount(manCtxs, 0, 0) != 1) Debug.Log("error");
        if (Ctx.TEST__GetPonCount(manCtxs, 0, 1) != 1) Debug.Log("error");
        if (Ctx.TEST__GetPonCount(manCtxs, 0, 2) != 1) Debug.Log("error");

        // (1,2,3) (1,2,3) (1,2,3)
        if (Ctx.TEST__GetChiCount(manCtxs, 1, 0) != 3) Debug.Log("error");
    }

    void Test3()
    {
        var testSet = new CardComponent[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1, 0),
                    TEST__SetTestData(TestComponents[1], "만", 1, 1),
                    TEST__SetTestData(TestComponents[2], "만", 2, 2),
                    TEST__SetTestData(TestComponents[3], "만", 2, 3),
                    TEST__SetTestData(TestComponents[4], "만", 3, 4),
                    TEST__SetTestData(TestComponents[5], "만", 3, 5),
                    TEST__SetTestData(TestComponents[6], "만", 3, 6),
                    TEST__SetTestData(TestComponents[7], "만", 4, 7),
                    TEST__SetTestData(TestComponents[8], "만", 5, 8),
                    TEST__SetTestData(TestComponents[9], "만", 6, 9),
                    TEST__SetTestData(TestComponents[10], "만", 7, 10),
                    TEST__SetTestData(TestComponents[11], "만", 8, 11),
                    TEST__SetTestData(TestComponents[12], "만", 8, 12),
                    TEST__SetTestData(TestComponents[13], "만", 8, 13),
        };
        var tiles = HandUtil.CardComponetsToIndexes(testSet);

        var resultCtxs = FindAll(tiles, null);

        // (1,2,3) (1,2,3) (3,4,5) (6,7,8)
        if (resultCtxs.Length != 1) Debug.Log("error");

        if (Ctx.TEST__GetChiCount(resultCtxs, 0, 0) != 2) Debug.Log("error");
        if (Ctx.TEST__GetChiCount(resultCtxs, 0, 2) != 1) Debug.Log("error");
        if (Ctx.TEST__GetChiCount(resultCtxs, 0, 5) != 1) Debug.Log("error");

        Ctx.PrintContexts(resultCtxs);
    }

    void Test4()
    {
        var testSet = new CardComponent[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 2, 0),
                    TEST__SetTestData(TestComponents[1], "만", 3, 1)
        };
        var chiTarget = TEST__SetTestData(TestComponents[2], "만", 1, 1);

        if (!IsChiable(testSet, chiTarget)) Debug.Log("error");

        chiTarget = TEST__SetTestData(TestComponents[2], "만", 4, 1);

        if (!IsChiable(testSet, chiTarget)) Debug.Log("error");
    }

    void Test5()
    {
        var testSet = new CardComponent[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 3, 0),
                    TEST__SetTestData(TestComponents[1], "만", 3, 1)
        };
        var ponTarget = TEST__SetTestData(TestComponents[2], "만", 3, 1);

        if (!IsPonable(testSet, ponTarget)) Debug.Log("error");
    }

    void Test6()
    {
        var testSet = new CardComponent[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1, 0),
                    TEST__SetTestData(TestComponents[1], "만", 1, 1),
                    TEST__SetTestData(TestComponents[2], "만", 2, 2),
                    TEST__SetTestData(TestComponents[3], "만", 2, 3),
                    TEST__SetTestData(TestComponents[4], "만", 3, 4),
                    TEST__SetTestData(TestComponents[5], "만", 3, 5),
                    TEST__SetTestData(TestComponents[6], "만", 4, 6),
                    TEST__SetTestData(TestComponents[7], "만", 4, 7),
                    TEST__SetTestData(TestComponents[8], "만", 5, 8),
                    TEST__SetTestData(TestComponents[9], "만", 5, 9),
                    TEST__SetTestData(TestComponents[10], "만", 6, 10),
                    TEST__SetTestData(TestComponents[11], "만", 6, 11),
                    TEST__SetTestData(TestComponents[12], "만", 7, 12),
                    TEST__SetTestData(TestComponents[13], "삭", 9, 13),
        };
        var tiles = HandUtil.CardComponetsToIndexes(testSet);

        if (!Chiitoitsu.IsTenpai(tiles)) Debug.Log("error");

        TEST__SetTestData(TestComponents[13], "만", 7, 13);
        tiles = HandUtil.CardComponetsToIndexes(testSet);

        if (!Chiitoitsu.IsWinable(tiles)) Debug.Log("error");
    }

    void Test7()
    {
        var testSet = new CardComponent[]
        {
                    TEST__SetTestData(TestComponents[0], "만", 1, 0),
                    TEST__SetTestData(TestComponents[1], "만", 9, 1),
                    TEST__SetTestData(TestComponents[2], "삭", 1, 2),
                    TEST__SetTestData(TestComponents[3], "삭", 9, 3),
                    TEST__SetTestData(TestComponents[4], "통", 1, 4),
                    TEST__SetTestData(TestComponents[5], "통", 9, 5),
                    TEST__SetTestData(TestComponents[6], "동", 1, 6),
                    TEST__SetTestData(TestComponents[7], "남", 2, 7),
                    TEST__SetTestData(TestComponents[8], "서", 3, 8),
                    TEST__SetTestData(TestComponents[9], "북", 4, 9),
                    TEST__SetTestData(TestComponents[10], "백", 5, 10),
                    TEST__SetTestData(TestComponents[11], "발", 6, 11),
                    TEST__SetTestData(TestComponents[12], "발", 6, 12),
                    TEST__SetTestData(TestComponents[13], "만", 1, 12),
        };
        var tiles = HandUtil.CardComponetsToIndexes(testSet);

        if (!Kokushimusou.IsTenpai(tiles)) Debug.Log("error");

        TEST__SetTestData(TestComponents[13], "중", 7, 13);
        tiles = HandUtil.CardComponetsToIndexes(testSet);

        if (!Kokushimusou.IsWinable(tiles)) Debug.Log("error");
    }


    public void Start()
    {
        if (IgnoreTests) { return; }

        Debug.Log($"--- HandCalculator TEST ---");

        TestComponents = GetComponentsInChildren<CardComponent>();

        Test1();
        Test2();
        Test3();
        Test4();
        Test5();
        Test6();
        Test7();

        Debug.Log("if nothing appeared above, test success");
    }

    CardComponent TEST__SetTestData(CardComponent card, string type, int cardNumber, int normalCardNumber)
    {
        card.Type = type;
        card.CardNumber = cardNumber;
        card.NormalCardNumber = normalCardNumber;
        return card;
    }
}