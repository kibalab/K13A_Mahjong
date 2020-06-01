
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputEvent : UdonSharpBehaviour
{
    // 모든 데이터는 Owner에게 전달하기 위해 UdonSynced여야 함
    [UdonSynced(UdonSyncMode.None)] public string EventType;
    [UdonSynced(UdonSyncMode.None)] public int PlayerIndex;
    [UdonSynced(UdonSyncMode.None)] public int CardIndex;

    public void Set(int cardIndex, string eventType, int playerIndex)
    {
        CardIndex = cardIndex;
        EventType = eventType;
        PlayerIndex = playerIndex;
    }

    public void Clear()
    {
        CardIndex = -1;
        EventType = "";
        PlayerIndex = -1;
    }
}
