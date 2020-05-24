using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CombinationIterator : UdonSharpBehaviour
{
    public bool IgnoreTests = false;

    int n;
    int[] combination;

    public void Start()
    {
        if (IgnoreTests) { return; }

        Debug.Log("--- CombinationIterator TEST ---");
        TestCombination(13, 3);
        TestCombination(5, 3);
        TestCombination(5, 2);
        TestCombination(5, 1);
        Debug.Log("if nothing appeared above, test success");
    }

    int GetEstimatedCount(int n, int k)
    {
        var estimatedCount = 1;
        for (var i = 0; i < k; ++i)
        {
            estimatedCount *= n--;
        }
        for (var i = k; i > 0; --i)
        {
            estimatedCount /= i;
        }
        return estimatedCount;
    }

    void TestCombination(int n, int k)
    {
        foreach (var obj in GetCombinationAll(n, k))
        {
            if (obj == null)
            {
                Debug.Log("error");
                break;
            }
        }
    }

    public object[] GetCombinationAll(int n, int k)
    {
        if (k == 0 || n < k)
        {
            return new object[0];
        }

        this.n = n;
        combination = new int[k];

        for (var i = 0; i < combination.Length; ++i)
        {
            combination[i] = i;
        }

        var objs = new object[GetEstimatedCount(n, k)];
        var count = 0;
        while (combination != null)
        {
            objs[count++] = GetHardCopy(combination);
            MoveNext();
        }

        return objs;
    }

    int[] GetCombination()
    {
        return combination;
    }

    void MoveNext()
    {
        ++combination[combination.Length - 1];

        for (var i = combination.Length - 1; i >= 0; --i)
        {
            var needCheckAgain = false;
            for (var sub = i; sub >= 0; --sub)
            {
                if (combination[sub] >= n)
                {
                    needCheckAgain = true;
                    if (sub == 0)
                    {
                        combination = null;
                        return;
                    }
                    ++combination[sub - 1];
                    for (var k = sub; k < combination.Length; ++k)
                    {
                        combination[k] = combination[k - 1] + 1;
                    }
                }
            }
            if (needCheckAgain) { i++; }
        }
    }

    int[] GetHardCopy(int[] raw)
    {
        var copies = new int[raw.Length];
        for (var i = 0; i < raw.Length; ++i)
        {
            copies[i] = raw[i];
        }
        return copies;
    }
}