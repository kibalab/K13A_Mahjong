using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class CombinationIterator : UdonSharpBehaviour
{
    int n;
    int[] combination;
    int iterTurn = 0;

    void Start()
    {
        Debug.Log("--- CombinationIterator TEST ---");
        TestCombination(13, 3);
        TestCombination(5, 3);
        TestCombination(5, 2);
    }

    void TestCombination(int n, int k)
    {
        Initialize(n, k);
        var estimatedCount = 1;
        for (var i = 0; i < k; ++i)
        {
            estimatedCount *= n--;
        }
        for (var i = k; i > 0; --i)
        {
            estimatedCount /= i;
        }

        var count = 0;
        while (GetCombination() != null)
        {
            MoveNext();
            count++;
        }

        Debug.Log("estimatedCount = " + estimatedCount + " calculatedCount = " + count);
    }


    public void Initialize(int n, int k)
    {
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
