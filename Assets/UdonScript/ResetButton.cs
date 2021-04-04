
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ResetButton : UdonSharpBehaviour
{
    public TableManager TB;
    public EventLog EventLog;
    public override void Interact()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ResetTable));
    }
    public void ResetTable()
    {
        TB.resetTable();
        EventLog.SetEvent("R");
    }
}
