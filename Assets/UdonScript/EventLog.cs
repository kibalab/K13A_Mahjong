
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventLog : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)]string eventName = "";
    public void SetEvent(string eventName)
    {
        this.eventName = eventName;
    }

    public string GetEvent()
    {
        var e = eventName;
        eventName = "";
        return e;
    }

    public bool IsEmpty()
    {
        return eventName == "";
    }
}
