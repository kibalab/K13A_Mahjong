
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventLogHandler : UdonSharpBehaviour
{
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public EventLogger EventLogger;
    [SerializeField] public KList events;

    [SerializeField] public TableManager TableManager;
    [SerializeField] public EventQueue EventQueue;

    [UdonSynced(UdonSyncMode.None)] string NetworkEvent = "";
    string ranNetworkEvent = "";
    int SentEventID = -1;

    bool Synced = false;

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player == Networking.LocalPlayer)
        {
            DoSync();
        }
        
    }

    public void DoSync()
    {
        Synced = false;

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(RequestSync));
    }

    private void Update()
    {
        if (!Networking.IsMaster && !Synced && NetworkEvent != ranNetworkEvent)
        {
            runEvent(NetworkEvent.Split('#')[1]);

            ranNetworkEvent = NetworkEvent;

            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(RequestNextEvent));
        }
    }

    public void RequestSync()
    {
        SentEventID = -1;
        RequestNextEvent();
    }

    public void RequestNextEvent()
    {
        if(string.IsNullOrWhiteSpace(EventLogger.GetEvent(SentEventID + 1)))
        {
            return;
        }
        SentEventID++;
        NetworkEvent = $"{SentEventID}#{EventLogger.GetEvent(SentEventID)}";
        LogViewer.Log($"직렬화 이벤트로그 송신 : {NetworkEvent}", 1);
    }


    public bool runEvent(string e)
    {
        LogViewer.Log($"직렬화 이벤트로그 실행 : {e}", 1);
        var parms = e.Split('&');
        switch (parms[0])
        {
            case "R": // Reset Game
                var seed = int.Parse(parms[1]);
                UnityEngine.Random.InitState(seed);
                TableManager.resetTable();
                return true;
            case "FI": //First Initialize
                UnityEngine.Random.InitState(int.Parse(parms[1]));
                return true;
            case "C": //Interect Card
                TableManager.CardPool.transform.Find($"Card ({parms[1]})").GetComponent<Card>().l_Interact();
                return true;
            case "IN": //Interect UI
                switch (parms[1])
                {
                    case "C":
                        EventQueue.SetChiEvent(StringToVector2(parms[2]), "Chi", int.Parse(parms[3]));
                        break;
                    case "P":
                        EventQueue.SetUIEvent("Pon", int.Parse(parms[2]));
                        break;
                    case "K":
                        EventQueue.SetUIEvent("Kkan", int.Parse(parms[2]));
                        break;
                    case "R":
                        EventQueue.SetUIEvent("Ron", int.Parse(parms[2]));
                        break;
                    case "T":
                        EventQueue.SetUIEvent("Tsumo", int.Parse(parms[2]));
                        break;
                    case "S":
                        EventQueue.SetUIEvent("Skip", int.Parse(parms[2]));
                        break;
                    case "RC":
                        EventQueue.SetUIEvent("Rich", int.Parse(parms[2]));
                        break;
                }
                return true;

            default:
                return false;
        }
    }


    public Vector2 StringToVector2(string sVector)
    {
        LogViewer.Log($"Vector2 읽는중 : {sVector}", 1);
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector2 result = new Vector2(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]));

        return result;
    }
}
