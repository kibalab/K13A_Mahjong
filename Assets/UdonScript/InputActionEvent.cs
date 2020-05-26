
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputActionEvent : UdonSharpBehaviour
{
    public CardComponent Card;
    [UdonSynced(UdonSyncMode.None)] public string EventType;
    public VRCPlayerApi Author;

    public void Initialize() {  }

    public void setData(CardComponent card, string eventType)
    {
        Card = card;
        EventType = eventType;
    }

    public void deleteData()
    {
        Card = null;
        EventType = null;
    }

}
