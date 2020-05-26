
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandCalculator : UdonSharpBehaviour
{
    const int MAN_START_INDEX = 0;
    const int MAN_END_INDEX = 8;
    const int PIN_START_INDEX = 9;
    const int PIN_END_INDEX = 17;
    const int SOU_START_INDEX = 18;
    const int SOU_END_INDEX = 26;
    const int WORDS_START_INDEX = 27;
    const int WORDS_END_INDEX = 33;

    public bool IgnoreTests = false;
    public CardComponent[] TestComponents;

    const int TILES_COUNT = 34;

    public KList Stack;
    public KList Result;
    public ContextHandler Ctx;

    public bool IsChiable(CardComponent[] cards, CardComponent discardedCard)
    {
        var tiles = CardComponetsToIndexes(cards);
        var chiIndex = CardComponentToIndex(discardedCard);

        // 자패는 치 가능하지 않음
        if (chiIndex >= WORDS_START_INDEX) { return false; }
        var startIndex = GetStartIndexOfType(discardedCard.Type);
        var endIndex = GetEndIndexOfType(discardedCard.Type);

        if (startIndex + 2 <= chiIndex && chiIndex <= endIndex - 0 && tiles[chiIndex - 2] > 0 && tiles[chiIndex - 1] > 0) return true;
        if (startIndex + 1 <= chiIndex && chiIndex <= endIndex - 1 && tiles[chiIndex - 1] > 0 && tiles[chiIndex + 1] > 0) return true;
        if (startIndex + 0 <= chiIndex && chiIndex <= endIndex - 2 && tiles[chiIndex + 1] > 0 && tiles[chiIndex + 2] > 0) return true;

        return false;
    }

    public bool IsPonable(CardComponent[] cards, CardComponent discardedCard)
    {
        var tiles = CardComponetsToIndexes(cards);
        var ponIndex = CardComponentToIndex(discardedCard);

        return tiles[ponIndex] + 1 >= 3;
    }

    public bool IsKkanable(CardComponent[] cards, CardComponent discardedCard)
    {
        var tiles = CardComponetsToIndexes(cards);
        var ponIndex = CardComponentToIndex(discardedCard);

        return tiles[ponIndex] + 1 == 4;
    }

    public int[] IsRiichiable(CardComponent[] cards)
    {
        if (cards.Length != 14) Debug.Log("cards.Length != 14");

        // 버려서 텐파이 되는 곳을 찾는다
        var riichiCreationIndex = new int[TILES_COUNT];

        var tiles = CardComponetsToIndexes(cards);
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
        // 텐파이란 무엇인가? 

        // check chiitoitus

        // check normal

        // check something speical



        return true;
    }

    //public bool IsNormalYaku(CardComponent[] cards)
    //{
    //    var tiles = CardComponetsToIndexes(cards);
    //    var dd = FindAll(tiles, new int[0]);
    //}

    object[] FindAll(int[] tiles, int[] openedTiles)
    {
        if (tiles.Length != TILES_COUNT) Debug.Log("TILES LENGTH ERROR!");

        Result.Clear();

        var maxChiPonCount = 0;
        foreach (var pairIndex in FindPairs(tiles))
        {
            var localTiles = Clone(tiles);
            localTiles[pairIndex] -= 2;

            var manCtxs = Find(localTiles, MAN_START_INDEX, MAN_END_INDEX);
            var pinCtxs = Find(localTiles, PIN_START_INDEX, PIN_END_INDEX);
            var souCtxs = Find(localTiles, SOU_START_INDEX, SOU_END_INDEX);

            // 111222333의 경우, 123/123/123과 111/222/333이 있을 수 있다
            // 따라서 (만의 슌커쯔)x(삭의 슌커쯔)x(통의 슌커쯔)가 경우의 수가 된다
            // 자패는 커쯔밖에 없으니 바뀔 일이 없음
            var manIndex = 0;
            do
            {
                object[] manCtx = manIndex < manCtxs.Length ? (object[])manCtxs[manIndex++] : null;
                var pinIndex = 0;
                do
                {
                    object[] pinCtx = pinIndex < pinCtxs.Length ? (object[])pinCtxs[pinIndex++] : null;
                    var souIndex = 0;
                    do
                    {
                        object[] souCtx = souIndex < souCtxs.Length ? (object[])souCtxs[souIndex++] : null;
                        var ctx = Ctx.AddContextAll(manCtx, pinCtx, souCtx);
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

                    } while (souCtxs.Length < souIndex);
                } while (pinCtxs.Length < pinIndex);
            } while (manCtxs.Length < manIndex);
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

    int[] FindPairs(int[] tiles)
    {
        // 머리를 찾는다. 최대 갯수는 14/2 7개
        var arr = new int[7];
        var index = 0;

        const int HONOR_INDEX_START = 27;
        for (var i = 0; i < tiles.Length; ++i)
        {
            // 커쯔인 자패는 무시한다. 머리로 빼봤자 나머지 하나가 역할을 못 함
            if (i >= HONOR_INDEX_START && tiles[i] != 2)
            {
                continue;
            }
            if (tiles[i] >= 2)
            {
                arr[index++] = i;
            }
        }
        return Fit(arr, index);
    }

    int[] CardComponetsToIndexes(CardComponent[] cards)
    {
        var tiles = new int[TILES_COUNT];
        foreach (var card in cards)
        {
            if (card != null)
            {
                tiles[CardComponentToIndex(card)]++;
            }
        }
        return tiles;
    }

    int CardComponentToIndex(CardComponent card)
    {
        // man 0-8
        // pin 9-17
        // sou 18-26
        // ewsn 27, 28, 29, 30
        // white green red 31, 32, 33
        var cardType = card != null ? card.Type : "INVAILD";
        switch (cardType)
        {
            case "만": return MAN_START_INDEX + (card.CardNumber - 1);
            case "통": return PIN_START_INDEX + (card.CardNumber - 1);
            case "삭": return SOU_START_INDEX + (card.CardNumber - 1);
            case "동": return 27;
            case "서": return 28;
            case "남": return 29;
            case "북": return 30;
            case "백": return 31;
            case "발": return 32;
            case "중": return 33;
        }
        return -1;
    }

    int GetStartIndexOfType(string cardType)
    {
        switch (cardType)
        {
            case "만": return MAN_START_INDEX;
            case "통": return PIN_START_INDEX;
            case "삭": return SOU_START_INDEX;
            case "동": return WORDS_START_INDEX;
            case "서": return WORDS_START_INDEX;
            case "남": return WORDS_START_INDEX;
            case "북": return WORDS_START_INDEX;
            case "백": return WORDS_START_INDEX;
            case "발": return WORDS_START_INDEX;
            case "중": return WORDS_START_INDEX;
        }
        return -1;
    }

    int GetEndIndexOfType(string cardType)
    {
        switch (cardType)
        {
            case "만": return MAN_END_INDEX;
            case "통": return PIN_END_INDEX;
            case "삭": return SOU_END_INDEX;
            case "동": return WORDS_END_INDEX;
            case "서": return WORDS_END_INDEX;
            case "남": return WORDS_END_INDEX;
            case "북": return WORDS_END_INDEX;
            case "백": return WORDS_END_INDEX;
            case "발": return WORDS_END_INDEX;
            case "중": return WORDS_END_INDEX;
        }
        return -1;
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

    int[] Fit(int[] arr, int count)
    {
        var newArr = new int[count];
        for (var i = 0; i < count; ++i)
        {
            newArr[i] = arr[i];
        }
        return newArr;
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

        var tiles = CardComponetsToIndexes(testSet);
        var manCtxs = Find(tiles, MAN_START_INDEX, MAN_END_INDEX);

        if (manCtxs.Length != 1) Debug.Log("error");

        // (1,2,3) (1,2,3)
        if (Ctx.TEST__GetChiCount(manCtxs, 0, 0) != 2) Debug.Log("error");
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

        var tiles = CardComponetsToIndexes(testSet);
        var manCtxs = Find(tiles, MAN_START_INDEX, MAN_END_INDEX);

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
        var tiles = CardComponetsToIndexes(testSet);

        var resultCtxs = FindAll(tiles, null);

        // (1,2,3) (1,2,3) (3,4,5) (6,7,8)
        if (resultCtxs.Length != 1) Debug.Log("error");

        if (Ctx.TEST__GetChiCount(resultCtxs, 0, 0) != 2) Debug.Log("error");
        if (Ctx.TEST__GetChiCount(resultCtxs, 0, 2) != 1) Debug.Log("error");
        if (Ctx.TEST__GetChiCount(resultCtxs, 0, 5) != 1) Debug.Log("error");

        // Ctx.PrintContexts(resultCtxs);
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