
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class SyncButton : UdonSharpBehaviour
{
    public EventLogHandler EventLogHandler;
    public override void Interact()
    {
        EventLogHandler.DoSync();
    }
}
