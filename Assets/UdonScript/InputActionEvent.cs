
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputActionEvent : UdonSharpBehaviour
{
    public CardComponent Card;
    [UdonSynced(UdonSyncMode.None)] public string EventType;
    [UdonSynced(UdonSyncMode.None)] public int playerTurn;
    public VRCPlayerApi Author;
    

    public void Initialize() {  }

    public void setData(CardComponent card, string eventType, int turn)
    {
        Card = card;
        EventType = eventType;
        playerTurn = turn;
    }

    public void deleteData()
    {
        Card = null;
        EventType = null;
    }

}
