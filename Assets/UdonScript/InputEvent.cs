
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputEvent : UdonSharpBehaviour
{
    public string EventType;
    public int PlayerIndex;
    public int DiscardedCardYamaIndex;
    public Vector2 ChiIndex;

    public void SetUIEvent(string uiName, int playerIndex)
    {
        EventType = uiName;
        PlayerIndex = playerIndex;
    }

    public void SetDiscardEvent(int yamaIndex, string eventType, int playerIndex)
    {
        DiscardedCardYamaIndex = yamaIndex;
        EventType = eventType;
        PlayerIndex = playerIndex;
    }

    public void SetChiEvent(Vector2 chiIndex, string eventType, int playerIndex)
    {
        ChiIndex = new Vector2(chiIndex.x, chiIndex.y);
        EventType = eventType;
        PlayerIndex = playerIndex;
    }

    public void Clear()
    {
        DiscardedCardYamaIndex = -1;
        EventType = "";
        PlayerIndex = -1;
        ChiIndex = Vector2.zero;
    }
}
