
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputEvent : UdonSharpBehaviour
{
    public bool isInputBlocked = false;
    public string EventType;
    public int PlayerIndex;
    public int DiscardedCardYamaIndex;
    public Vector2 ChiIndex;
    public string DrawReason;
    public VRCPlayerApi[] Players;

    public void Clear()
    {
        DiscardedCardYamaIndex = -1;
        EventType = "";
        PlayerIndex = -1;
        ChiIndex = Vector2.zero;
        DrawReason = "";
        Players = null;
    }
}
