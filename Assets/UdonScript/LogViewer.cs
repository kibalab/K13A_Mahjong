
using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class LogViewer : UdonSharpBehaviour
{
    public Text debugText1;
    public Text debugText2;
    public void Log(string str, int logViewer)
    {
        str = AddTimestamp(str);

        switch (logViewer)
        {
            case 0:
                debugText1.text += "\n" + str;
                Debug.Log("[LogViewer1] " + str);
                break;
            case 1:
                debugText2.text += "\n" + str;
                Debug.Log("[LogViewer2] " + str);
                break;
        }
    }
    public void ErrorLog(string str, int logViewer)
    {
        str = AddTimestamp(str);

        switch (logViewer)
        {
            case 0:
                debugText1.text += "\n<color=red>" + str + "</color>";
                Debug.Log("[LogViewer1 Error] " + str);
                break;
            case 1:
                debugText2.text += "\n<color=red>" + str + "</color>";
                Debug.Log("[LogViewer2 Error] " + str);
                break;
        }
    }

    string AddTimestamp(string str)
    {
        var dateTime = DateTime.Now.ToString("HH:mm:ss");
        return $"[{dateTime}]: {str}";
    }
}
