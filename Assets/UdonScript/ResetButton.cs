
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ResetButton : UdonSharpBehaviour
{
    public GameManager GameManager;
    public EventLog EventLog;

    TableManager TB;

    private void Start()
    {
        TB = GameManager.TableManager;
    }
    public override void Interact()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(ResetTable));
    }
    public void ResetTable()
    {
        GameManager.Seed = UnityEngine.Random.Range(1, 2147483647);

        EventLog.SetEvent("R&" + GameManager.Seed);
    }
}
