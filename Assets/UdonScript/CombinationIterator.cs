using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class CombinationIterator : UdonSharpBehaviour
{
    public bool IgnoreTests = false;

    int n;
    int[] combination;

    void Start()
    {
        if (IgnoreTests) { return; }

        Debug.Log("--- CombinationIterator TEST ---");
        TestCombination(13, 3);
        TestCombination(5, 3);
        TestCombination(5, 2);
        TestCombination(5, 1);
        Debug.Log("if nothing appeared, test success");
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
        Initialize(n, k);

        var count = 0;
        while (GetCombination() != null)
        {
            MoveNext();
            count++;
        }

        if (GetEstimatedCount(n, k) != count) Debug.Log("Estmation Error");
    }

    public void Initialize(int n, int k)
    {
        if (k == 0 || n < k)
        {
            combination = null;
            return;
        }

        this.n = n;
        combination = new int[k];

        for (var i = 0; i < combination.Length; ++i)
        {
            combination[i] = i;
        }
    }

    public int[] GetCombination()
    {
        return combination;
    }

    public void MoveNext()
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
}
