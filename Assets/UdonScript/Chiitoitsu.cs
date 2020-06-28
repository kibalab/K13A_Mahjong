
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Chiitoitsu : UdonSharpBehaviour
{
    [SerializeField] public HandUtil HandUtil;

    public bool CheckTenpai(AgariContext agariContext, int[] globalOrders)
    {
        var pairs = HandUtil.FindPairs(globalOrders);
        if (pairs.Length != 6)
        {
            return false;
        }

        for (var i = 0; i < pairs.Length; ++i)
        {
            globalOrders[pairs[i]] -= 2;
        }

        for (var i = 0; i < globalOrders.Length; ++i)
        {
            if (globalOrders[i] > 0)
            {
                agariContext.AddAgariableGlobalOrder(i);
            }
        }

        return true;
    }
}