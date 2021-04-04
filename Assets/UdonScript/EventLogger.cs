
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventLogger : UdonSharpBehaviour
{
    public string SerializedEvents = "";
    public LogViewer LogViewer;
    public EventLog EventLog;
    public KList events;

    public void AddEventLog(string newEvent)
    {
        events.Add(newEvent);
        SaveSerializedEvents();
    }

    public void DeleteToLastResetEvent()
    {
        var e = events.RemoveLast();
        if ((string)e == "Reset" || e == "FI")
        {
            events.Add(e);
        }
        SaveSerializedEvents();
    }
    public string SaveSerializedEvents()
    {
        SerializedEvents = "";
        foreach (object e in events.Clone())
        {
            SerializedEvents += (string)e + "/";
        }
        LogViewer.Log($"직렬화 이벤트로그 : {SerializedEvents}", 1);
        return SerializedEvents;
    }

    public void Clear()
    {
        SerializedEvents = "";
        events.Clear();
    }

    public string GetEvent(int i) => (string)events.At(i);
    public int GetCount() => events.Count();

    private void Update()
    {
        if (!EventLog.IsEmpty())
        {
            AddEventLog(EventLog.GetEvent());
        }
    }
}
