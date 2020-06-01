
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CalculatingContextHandler : UdonSharpBehaviour
{
    // 이 클래스는 static class처럼 씁니다. 별도의 멤버변수를 갖게 하지 맙시다

    const int MAN_START_INDEX = 0;
    const int MAN_END_INDEX = 8;
    const int PIN_START_INDEX = 9;
    const int PIN_END_INDEX = 17;
    const int SOU_START_INDEX = 18;
    const int SOU_END_INDEX = 26;

    const int TILES = 0;
    const int CHILIST = 1;
    const int CHICOUNT = 2;
    const int PONLIST = 3;
    const int PONCOUNT = 4;

    public object[] CreateContext(int[] typedTiles)
    {
        var context = new object[5];
        context[TILES] = Clone(typedTiles);
        context[CHILIST] = new int[10] { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };
        context[CHICOUNT] = 0;
        context[PONLIST] = new int[10] { 999, 999, 999, 999, 999, 999, 999, 999, 999, 999 };
        context[PONCOUNT] = 0;
        return context;
    }

    public object[] CopyContext(object[] context)
    {
        var newContext = new object[5];
        newContext[TILES] = Clone(ReadTiles(context));
        newContext[CHILIST] = Clone(ReadChiList(context));
        newContext[CHICOUNT] = ReadChiCount(context);
        newContext[PONLIST] = Clone(ReadPonList(context));
        newContext[PONCOUNT] = ReadPonCount(context);
        return newContext;
    }

    public object[] AddContextAll(object[] ctx1, object[] ctx2, object[] ctx3, object[] ctx4)
    {
        var added1 = AddContext(ctx1, ctx2);
        var added2 = AddContext(ctx3, ctx4);
        return AddContext(added1, added2);
    }

    public object[] AddContext(object[] ctx1, object[] ctx2)
    {
        if (ctx1 == null) { return ctx2; }
        if (ctx2 == null) { return ctx1; }
        if (ctx1 == null && ctx2 == null) { return null; }

        var context = CreateContext(new int[34]);
        var newTiles = ReadTiles(context);
        var ctx1Tiles = ReadTiles(ctx1);
        var ctx2Tiles = ReadTiles(ctx2);

        for (var i = 0; i < 34; ++i)
        {
            newTiles[i] = ctx1Tiles[i] + ctx2Tiles[i];
        }
        var ctx1Chis = ReadChiList(ctx1);
        for (var i = 0; i < ReadChiCount(ctx1); ++i)
        {
            AppendChiList(context, ctx1Chis[i]);
        }
        var ctx2Chis = ReadChiList(ctx2);
        for (var i = 0; i < ReadChiCount(ctx2); ++i)
        {
            AppendChiList(context, ctx2Chis[i]);
        }
        var ctx1Pons = ReadPonList(ctx1);
        for (var i = 0; i < ReadPonCount(ctx1); ++i)
        {
            AppendPonList(context, ctx1Pons[i]);
        }
        var ctx2Pons = ReadPonList(ctx2);
        for (var i = 0; i < ReadPonCount(ctx2); ++i)
        {
            AppendPonList(context, ctx2Pons[i]);
        }

        return context;
    }

    public int[] ReadTiles(object[] context)
    {
        return (int[])context[TILES];
    }

    public int[] ReadChiList(object[] context)
    {
        return (int[])context[CHILIST];
    }

    public int ReadChiCount(object[] context)
    {
        return (int)context[CHICOUNT];
    }

    public int[] ReadPonList(object[] context)
    {
        return (int[])context[PONLIST];
    }

    public int ReadPonCount(object[] context)
    {
        return (int)context[PONCOUNT];
    }

    public void ApplyPon(object[] context, int startIndex)
    {
        var tiles = ReadTiles(context);
        tiles[startIndex] -= 3;

        AppendPonList(context, startIndex);
    }

    public void ApplyChi(object[] context, int startIndex)
    {
        var tiles = ReadTiles(context);
        tiles[startIndex]--;
        tiles[startIndex + 1]--;
        tiles[startIndex + 2]--;

        AppendChiList(context, startIndex);
    }

    public void AppendPonList(object[] context, int ponIndex)
    {
        var ponList = ReadPonList(context);
        var ponCount = ReadPonCount(context);

        ponList[ponCount++] = ponIndex;
        context[PONCOUNT] = ponCount;
        Sort(ponList, ponCount);
    }

    public void AppendChiList(object[] context, int chiStartIndex)
    {
        var chiList = ReadChiList(context);
        var chiCount = ReadChiCount(context);

        chiList[chiCount++] = chiStartIndex;
        context[CHICOUNT] = chiCount;
        Sort(chiList, chiCount);
    }

    public bool IsRemainsChiCountEqual(object[] context, int startIndex)
    {
        var tiles = ReadTiles(context);
        return tiles[startIndex] == tiles[startIndex + 1]
            && tiles[startIndex + 1] == tiles[startIndex + 2];
    }

    public object[] CloneObject(object[] arr)
    {
        var clone = new object[arr.Length];
        for (var i = 0; i < arr.Length; ++i)
        {
            clone[i] = arr[i];
        }
        return clone;
    }

    public int[] Clone(int[] arr)
    {
        var clone = new int[arr.Length];
        for (var i = 0; i < arr.Length; ++i)
        {
            clone[i] = arr[i];
        }
        return clone;
    }

    public void PrintContexts(object[] contexts)
    {
        foreach (object[] context in contexts)
        {
            PrintContext(context);
        }
    }

    public void PrintContext(object[] context)
    {
        var tiles = ReadTiles(context);
        var chi = ReadChiList(context);
        var chiCount = ReadChiCount(context);
        var pon = ReadPonList(context);
        var ponCount = ReadPonCount(context);

        var str = "";
        for (var i = 0; i < chiCount; ++i)
        {
            var index = chi[i];
            var cardNumber = index % 9 + 1;
            var type = IndexToType(index);
            str += $"({type + (cardNumber)}, {type + (cardNumber + 1)}, {type + (cardNumber + 2)})";
        }
        for (var i = 0; i < ponCount; ++i)
        {
            var index = pon[i];
            var cardNumber = index % 9 + 1;
            var type = IndexToType(index);
            str += $"({type + (cardNumber)}, {type + (cardNumber)}, {type + (cardNumber)})";
        }
        str += " ";
        for (var i = 0; i < tiles.Length; ++i)
        {
            for (var k = 0; k < tiles[i]; ++k)
            {
                var index = i;
                var cardNumber = index % 9 + 1;
                var type = IndexToType(index);
                str += $"({type + (cardNumber)})";
            }
        }

        if (!string.IsNullOrEmpty(str))
        {
            Debug.Log(str);
        }
    }

    public int[] Fit(int[] arr, int count)
    {
        var newArr = new int[count];
        for (var i = 0; i < count; ++i)
        {
            newArr[i] = arr[i];
        }
        return newArr;
    }

    public object[] FitObject(object[] arr, int count)
    {
        var newArr = new object[count];
        for (var i = 0; i < count; ++i)
        {
            newArr[i] = arr[i];
        }
        return newArr;
    }

    public bool HasSameTiles(object[] ctxs, object[] ctx2)
    {
        var tiles2 = ReadTiles(ctx2);

        foreach (object[] ctx1 in ctxs)
        {
            var tiles1 = ReadTiles(ctx1);
            if (HasSameValue(tiles1, tiles1.Length, tiles2, tiles2.Length))
            {
                return true;
            }
        }

        return false;
    }

    public bool HasSameChiPons(object[] ctxs, object[] ctx2)
    {
        foreach (object[] ctx1 in ctxs)
        {
            var chiIsSame = HasSameValue(ReadChiList(ctx1), ReadChiCount(ctx1), ReadChiList(ctx2), ReadChiCount(ctx2));
            var ponIsSame = HasSameValue(ReadPonList(ctx1), ReadPonCount(ctx1), ReadPonList(ctx2), ReadPonCount(ctx2));
            if (chiIsSame && ponIsSame)
            {
                return true;
            }
        }

        return false;
    }

    bool HasSameValue(int[] arr1, int arr1Count, int[] arr2, int arr2Count)
    {
        if (arr1Count != arr2Count) { return false; }

        for (var i = 0; i < arr1Count; ++i)
        {
            if (arr1[i] != arr2[i])
            {
                return false;
            }
        }

        return true;
    }

    void Sort(int[] arr, int count)
    {
        for (var i = count; i >= 0; i--)
        {
            for (var j = 1; j <= i; j++)
            {
                if (arr[j - 1] > arr[j])
                {
                    var temp = arr[j - 1];
                    arr[j - 1] = arr[j];
                    arr[j] = temp;
                }
            }
        }
    }

    public object[] AddNullForward(object[] ctxs)
    {
        var newArr = new object[ctxs.Length + 1];
        newArr[0] = null;
        for (var i = 0; i < ctxs.Length; ++i)
        {
            newArr[i + 1] = ctxs[i];
        }
        return newArr;
    }

    string IndexToType(int index)
    {
        if (MAN_START_INDEX <= index && index <= MAN_END_INDEX) return "만";
        if (PIN_START_INDEX <= index && index <= PIN_END_INDEX) return "통";
        if (SOU_START_INDEX <= index && index <= SOU_END_INDEX) return "삭";
        if (index == 27) return "동";
        if (index == 28) return "서";
        if (index == 29) return "남";
        if (index == 30) return "북";
        if (index == 31) return "백";
        if (index == 32) return "발";
        if (index == 33) return "중";
        return "";
    }

    public int TEST__GetChiCount(object[] contexts, int ctxIndex, int chiIndex)
    {
        var count = 0;
        var context = (object[])contexts[ctxIndex];
        var chi = ReadChiList(context);
        for (var i = 0; i < ReadChiCount(context); ++i)
        {
            if (chi[i] == chiIndex)
            {
                count++;
            }
        }
        return count;
    }

    public int TEST__GetPonCount(object[] contexts, int ctxIndex, int ponIndex)
    {
        var count = 0;
        var context = (object[])contexts[ctxIndex];
        var pon = ReadPonList(context);
        for (var i = 0; i < ReadPonCount(context); ++i)
        {
            if (pon[i] == ponIndex)
            {
                count++;
            }
        }
        return count;
    }
}