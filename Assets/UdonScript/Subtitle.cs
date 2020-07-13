
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

//게임 마스터가 자막 표시를 명령하는부분
public class Subtitle : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public bool showRequest = false;
    [UdonSynced(UdonSyncMode.None)] public string name;
    [UdonSynced(UdonSyncMode.None)] public string text;
    [UdonSynced(UdonSyncMode.None)] public float playtime = 3.0f;


    public SubtitleComponent[] components;


    public void setSubtitle(string name, string text)
    {
        this.name = name;
        this.text = text;
        showRequest = true;

    }

    public void setPlaytime(float playtime)
    {
        this.playtime = playtime;
    }

    public void _ReqConfirm()
    {
        showRequest = false;
    }

    void Update()
    {
        if (!showRequest) return;

        components = gameObject.GetComponentsInChildren<SubtitleComponent>();
        components[components.Length - 1].Set(name, text, playtime);

        

        RequestCallFunctionToOwner(nameof(_ReqConfirm));
    }

    void RequestCallFunctionToOwner(string funcName)
    {
        if (Networking.LocalPlayer == null)
        {
            SendCustomEvent(funcName);
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, funcName);
        }
    }
}
