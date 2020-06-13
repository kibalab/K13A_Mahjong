
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputEvent : UdonSharpBehaviour
{
    // 모든 데이터는 Owner에게 전달하기 위해 UdonSynced여야 함
    [UdonSynced(UdonSyncMode.None)] public string EventType;
    [UdonSynced(UdonSyncMode.None)] public int PlayerIndex;
    [UdonSynced(UdonSyncMode.None)] public int DiscardedCardYamaIndex;

    // 여러개의 Chi중 선택한 치를 담는변수
    [UdonSynced(UdonSyncMode.None)] public Vector2 ChiIndex;

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
