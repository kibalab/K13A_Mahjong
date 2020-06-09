
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandUtil : UdonSharpBehaviour
{
    private const int MAN_START_GLOBAL_ORDER = 0;
    private const int MAN_END_GLOBAL_ORDER = 8;
    private const int PIN_START_GLOBAL_ORDER = 9;
    private const int PIN_END_GLOBAL_ORDER = 17;
    private const int SOU_START_GLOBAL_ORDER = 18;
    private const int SOU_END_GLOBAL_ORDER = 26;
    private const int WORDS_START_GLOBAL_ORDER = 27;
    private const int WORDS_END_GLOBAL_ORDER = 33;
    private const int TILES_COUNT = 34;

    public int GetManStartGlobalOrder()
    {
        return MAN_START_GLOBAL_ORDER;
    }

    public int GetManEndGlobalOrder()
    {
        return MAN_END_GLOBAL_ORDER;
    }

    public int GetPinStartGlobalOrder()
    {
        return PIN_START_GLOBAL_ORDER;
    }

    public int GetPinEndGlobalOrder()
    {
        return PIN_END_GLOBAL_ORDER;
    }

    public int GetSouStartGlobalOrder()
    {
        return SOU_START_GLOBAL_ORDER;
    }

    public int GetSouEndGlobalOrder()
    {
        return SOU_END_GLOBAL_ORDER;
    }

    public int GetWordsStartGlobalOrder()
    {
        return WORDS_START_GLOBAL_ORDER;
    }

    public int GetWordsEndGlobalOrder()
    {
        return WORDS_END_GLOBAL_ORDER;
    }

    public int GetEastCardNumber()
    {
        return 1;
    }

    public int GetSouthCardNumber()
    {
        return 2;
    }

    public int GetWestCardNumber()
    {
        return 3;
    }

    public int GetNorthCardNumber()
    {
        return 4;
    }

    public int GetWhiteCardNumber()
    {
        return 5;
    }

    public int GetGreenCardNumber()
    {
        return 6;
    }

    public int GetRedCardNumber()
    {
        return 7;
    }

    public int GetCardsCount(int[] globalOrders)
    {
        var count = 0;
        for (var i = 0; i < globalOrders.Length; ++i)
        {
            count += globalOrders[i];
        }
        return count;
    }

    public int GetWordsCardTypeCount(int[] globalOrders)
    {
        var count = 0;
        for (var i = WORDS_START_GLOBAL_ORDER; i <= WORDS_END_GLOBAL_ORDER; ++i)
        {
            if (globalOrders[i] > 0)
            {
                count++;
            }
        }
        return count;
    }

    public int GetYaojuhaiTypeCount(int[] tiles)
    {
        return (tiles[MAN_START_GLOBAL_ORDER] > 0 ? 1 : 0)
             + (tiles[MAN_END_GLOBAL_ORDER] > 0 ? 1 : 0)
             + (tiles[SOU_START_GLOBAL_ORDER] > 0 ? 1 : 0)
             + (tiles[SOU_END_GLOBAL_ORDER] > 0 ? 1 : 0)
             + (tiles[PIN_START_GLOBAL_ORDER] > 0 ? 1 : 0)
             + (tiles[PIN_END_GLOBAL_ORDER] > 0 ? 1 : 0)
             + GetWordsCardTypeCount(tiles);
    }

    public int[] GetGlobalOrders(Card[] cards)
    {
        var globalOrders = new int[TILES_COUNT];
        foreach (var card in cards)
        {
            if (card != null)
            {
                globalOrders[card.GlobalOrder]++;
            }
        }
        return globalOrders;
    }

    public int GetGlobalOrder(string cardType, int cardNumber)
    {
        return GetStartGlobalOrderOf(cardType) + (cardNumber - 1);
    }

    public int GetStartGlobalOrderOf(string cardType)
    {
        switch (cardType)
        {
            case "만": return MAN_START_GLOBAL_ORDER;
            case "통": return PIN_START_GLOBAL_ORDER;
            case "삭": return SOU_START_GLOBAL_ORDER;
            case "동": return WORDS_START_GLOBAL_ORDER;
            case "서": return WORDS_START_GLOBAL_ORDER;
            case "남": return WORDS_START_GLOBAL_ORDER;
            case "북": return WORDS_START_GLOBAL_ORDER;
            case "백": return WORDS_START_GLOBAL_ORDER;
            case "발": return WORDS_START_GLOBAL_ORDER;
            case "중": return WORDS_START_GLOBAL_ORDER;
        }
        return -1;
    }

    public int GetEndGlobalOrderOf(string cardType)
    {
        switch (cardType)
        {
            case "만": return MAN_END_GLOBAL_ORDER;
            case "통": return PIN_END_GLOBAL_ORDER;
            case "삭": return SOU_END_GLOBAL_ORDER;
            case "동": return WORDS_END_GLOBAL_ORDER;
            case "서": return WORDS_END_GLOBAL_ORDER;
            case "남": return WORDS_END_GLOBAL_ORDER;
            case "북": return WORDS_END_GLOBAL_ORDER;
            case "백": return WORDS_END_GLOBAL_ORDER;
            case "발": return WORDS_END_GLOBAL_ORDER;
            case "중": return WORDS_END_GLOBAL_ORDER;
        }
        return -1;
    }

    public int[] FindPairs(int[] globalOrders)
    {
        // 머리를 찾는다. 최대 갯수는 14/2 7개
        // 인데 버그나서 임시로 늘려봄;;;
        var arr = new int[14];
        var index = 0;

        for (var i = 0; i < globalOrders.Length; ++i)
        {
            // 커쯔인 자패는 무시한다. 머리로 빼봤자 나머지 하나가 역할을 못 함
            if (i >= WORDS_START_GLOBAL_ORDER && globalOrders[i] != 2)
            {
                continue;
            }
            if (globalOrders[i] >= 2)
            {
                arr[index++] = i;
            }
        }
        return Fit(arr, index);
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
}