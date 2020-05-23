using UdonSharp;

public class CombinationIterator : UdonSharpBehaviour
{
    int n;
    int[] combination;
    int iterTurn = 0;

    public void Initialize(int n, int k)
    {
        this.n = n;
        combination = new int[k];

        for (var i = 0; i < combination.Length; ++i)
        {
            combination[i] = i;
        }
        iterTurn = 0;
    }

    public int[] NextCombination()
    {
        if (iterTurn > 0)
        {
            ++combination[combination.Length - 1];
        }
        ++iterTurn;

        if (IsSatisfyCondition(0))
        {
            return combination;
        }
        else return null;
    }

    bool IsSatisfyCondition(int i)
    {
        if (i == combination.Length - 1)
        {
            return combination[i] < n;
        }

        if (!IsSatisfyCondition(i + 1))
        {
            SetValueRecursive(i, combination[i] + 1);
            return IsSatisfyCondition(i + 1);
        }

        return combination[i] < n;
    }

    void SetValueRecursive(int i, int val)
    {
        if (i < combination.Length)
        {
            combination[i] = val;
            SetValueRecursive(i + 1, val + 1);
        }
    }
}
