
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

    int lineCount1 = 0, lineCount2 = 0;
    public void Log(string str, int logViewer)
    {
        str = AddTimestamp(str);

        switch (logViewer)
        {
            case 0:
                debugText1.text += $"\n{str}";
                Debug.Log("[LogViewer1] " + str);
                lineCount1++;
                break;
            case 1:
                debugText2.text += $"\n{str}";
                Debug.Log("[LogViewer2] " + str);
                lineCount2++;
                break;
        }

        DeleteOldLog();
    }

    public void ErrorLog(string str, int logViewer)
    {
        str = AddTimestamp(str);

        switch (logViewer)
        {
            case 0:
                debugText1.text += $"\n<color=red>{str}</color>";
                debuglog("[LogViewer1 Error] " + str);
                break;
            case 1:
                debugText2.text += $"\n<color=red>{str}</color>";
                debuglog("[LogViewer2 Error] " + str);
                break;
        }
        DeleteOldLog();
    }

    string AddTimestamp(string str)
    {
        var dateTime = DateTime.Now.ToString("HH:mm:ss");
        return $"[<color=green>{dateTime}</color>] {str}";
    }

    void debuglog(string str)
    {
        if (Networking.LocalPlayer != null)
        {
            return;
        }
        Debug.Log(str);
    }

    void DeleteOldLog()
    {

        if (lineCount1 > 24)
        {
            var text = debugText1.text;
            debugText1.text = text.Substring(text.IndexOf('\n') + 1, text.Length - text.IndexOf('\n') - 1);
            lineCount1--;
        }

        if (lineCount2 > 24)
        {
            var text = debugText2.text;
            debugText2.text = text.Substring(text.IndexOf('\n') + 1, text.Length - text.IndexOf('\n') - 1);
            lineCount2--;
        }
    }
}
