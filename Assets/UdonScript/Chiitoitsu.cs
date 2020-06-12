
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Chiitoitsu : UdonSharpBehaviour
{
    [SerializeField] public HandUtil HandUtil;

    public bool IsTenpai(int[] tiles)
    {
        var pairs = HandUtil.FindPairs(tiles);
        return pairs.Length == 6;
    }

    public bool IsWinable(int[] tiles)
    {
        var pairs = HandUtil.FindPairs(tiles);
        return pairs.Length == 7;
    }
}