
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Kokushimusou : UdonSharpBehaviour
{
    public HandUtil HandUtil;

    public bool IsTenpai(int[] tiles)
    {
        var count = HandUtil.GetYaojuhaiTypeCount(tiles);
        return count == 12;
    }

    public bool IsWinable(int[] tiles)
    {
        var count = HandUtil.GetYaojuhaiTypeCount(tiles);
        return count == 13;
    }
}