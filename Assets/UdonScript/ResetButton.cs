
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
        GameManager.seed = UnityEngine.Random.Range(1, 2147483647);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ResetTable));
    }
    public void ResetTable()
    {
        UnityEngine.Random.InitState(GameManager.seed);
        TB.resetTable();

        EventLog.SetEvent("R&" + GameManager.seed);
    }
}
