
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Kokushimusou : UdonSharpBehaviour
{
    public HandUtil HandUtil;

    public bool CheckTenpai(AgariContext agariContext, int[] globalOrders)
    {
        var count = HandUtil.GetYaojuhaiTypeCount(globalOrders);
        if (count > 12)
        {
            // 국사무쌍 13면팅은 어떻게 할까?
            var isKokushiMusou13MenMach = count == 13;
            var pair = HandUtil.FindPairs(globalOrders);
            var exceptGlobalOrder = pair.Length != 0 ? pair[0] : -1;

            foreach(var globalOrder in HandUtil.GetYaojuhaiGlobalOrders())
            {
                if (globalOrder != exceptGlobalOrder)
                {
                    agariContext.AddAgariableGlobalOrder(globalOrder);
                }
            }

            return true;
        }
        return false;
    }

    public bool IsTenpai(int[] tiles)
    {
        var count = HandUtil.GetYaojuhaiTypeCount(tiles);
        return count >= 12;
    }

    public bool IsWinable(int[] tiles)
    {
        var count = HandUtil.GetYaojuhaiTypeCount(tiles);
        return count == 13;
    }
}