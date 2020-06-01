
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputEvent : UdonSharpBehaviour
{
    // ��� �����ʹ� Owner���� �����ϱ� ���� UdonSynced���� ��
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
