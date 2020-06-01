
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandUtil : UdonSharpBehaviour
{
    private const int MAN_START_INDEX = 0;
    private const int MAN_END_INDEX = 8;
    private const int PIN_START_INDEX = 9;
    private const int PIN_END_INDEX = 17;
    private const int SOU_START_INDEX = 18;
    private const int SOU_END_INDEX = 26;
    private const int WORDS_START_INDEX = 27;
    private const int WORDS_END_INDEX = 33;
    private const int TILES_COUNT = 34;

    public int GetManStartIndex()
    {
        return MAN_START_INDEX;
    }

    public int GetManEndIndex()
    {
        return MAN_END_INDEX;
    }

    public int GetPinStartIndex()
    {
        return PIN_START_INDEX;
    }

    public int GetPinEndIndex()
    {
        return PIN_END_INDEX;
    }

    public int GetSouStartIndex()
    {
        return SOU_START_INDEX;
    }

    public int GetSouEndIndex()
    {
        return SOU_END_INDEX;
    }

    public int GetWordsStartIndex()
    {
        return WORDS_START_INDEX;
    }

    public int GetWordsEndIndex()
    {
        return WORDS_END_INDEX;
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

    public int GetCardsCount(int[] tiles)
    {
        var count = 0;
        for (var i = 0; i < tiles.Length; ++i)
        {
            count += tiles[i];
        }
        return count;
    }

    public int GetWordsCardTypeCount(int[] tiles)
    {
        var count = 0;
        for (var i = WORDS_START_INDEX; i <= WORDS_END_INDEX; ++i)
        {
            if (tiles[i] > 0)
            {
                count++;
            }
        }
        return count;
    }

    public int GetYaojuhaiTypeCount(int[] tiles)
    {
        return (tiles[MAN_START_INDEX] > 0 ? 1 : 0)
             + (tiles[MAN_END_INDEX] > 0 ? 1 : 0)
             + (tiles[SOU_START_INDEX] > 0 ? 1 : 0)
             + (tiles[SOU_END_INDEX] > 0 ? 1 : 0)
             + (tiles[PIN_START_INDEX] > 0 ? 1 : 0)
             + (tiles[PIN_END_INDEX] > 0 ? 1 : 0)
             + GetWordsCardTypeCount(tiles);
    }

    public int[] CardComponetsToIndexes(Card[] cards)
    {
        var tiles = new int[TILES_COUNT];
        foreach (var card in cards)
        {
            if (card != null)
            {
                tiles[card.GlobalIndex]++;
            }
        }
        return tiles;
    }

    public int CardComponentToIndex(string cardType, int cardNumber)
    {
        return GetStartIndexOfType(cardType) + (cardNumber - 1);
    }

    public int GetStartIndexOfType(string cardType)
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

    public int GetEndIndexOfType(string cardType)
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

    public int[] FindPairs(int[] tiles)
    {
        // 머리를 찾는다. 최대 갯수는 14/2 7개
        var arr = new int[7];
        var index = 0;

        for (var i = 0; i < tiles.Length; ++i)
        {
            // 커쯔인 자패는 무시한다. 머리로 빼봤자 나머지 하나가 역할을 못 함
            if (i >= WORDS_START_INDEX && tiles[i] != 2)
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