
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputActionEvent : UdonSharpBehaviour
{
    public Card Card;
    [UdonSynced(UdonSyncMode.None)] public string EventType;
    [UdonSynced(UdonSyncMode.None)] public int playerTurn;
    public VRCPlayerApi Author;
    

    public void Initialize() {  }

    public void setData(Card card, string eventType, int turn)
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
