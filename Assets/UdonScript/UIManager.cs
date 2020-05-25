
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIManager : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public int playerId = 0;
    [UdonSynced(UdonSyncMode.None)] public string UIName;

    public GameObject UICanvas;

    public void Initialize()
    {
        // EventQueue가져와서 클릭했을때 이벤트를 넘길려했는데
        // 인수와 반환이 CardComponent라서 더미 카드컴포넌트 만들기도 쫌 그래서 냅둠
    }

    public void toggleUI(string _UIName, VRCPlayerApi player)
    {
        UIName = _UIName;
        playerId = player.playerId;

        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "_toogleUI");
    }

    public void _toogleUI()
    {
        if (Networking.LocalPlayer.playerId == playerId)
        {
            UICanvas.transform.Find(UIName);
        }
    }

    public void ClickButton()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }

    public void _ClickButton()
    {
        findPlayer();
        //여따가 이벤트
    }

    public VRCPlayerApi findPlayer()
    {
        return VRCPlayerApi.GetPlayerById(playerId);
    }
}
