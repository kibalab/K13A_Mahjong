
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class UIManager : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public int playerId = -1;
    [UdonSynced(UdonSyncMode.None)] public string UIName;

    public GameObject UICanvas;
    public Button Pon, Chi, Kkan, Rich, Ron, Tsumo, Skip;

    private InputActionEvent inputEvent;

    public void Initialize(InputActionEvent e)
    {
        inputEvent = e;
    }

    public void ActiveButton(string _UIName, VRCPlayerApi player)
    {
        Debug.Log("[UION] Player id : " + playerId);
        UIName = _UIName;
        if (player != null)
        {
            playerId = player.playerId;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "_ActiveUI");
        } else
        {
            playerId = -1;
            _ActiveButton();
        }
    }

    public void _ActiveButton()
    {
        if (playerId != -1)//유니티에서 테스트를 위한 조건문
        {
            if (Networking.LocalPlayer.playerId == playerId)
            {
                GameObject g = UICanvas.transform.Find(UIName).gameObject;
                UICanvas.SetActive(true);
                g.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.Log("[UION] Player id : " + playerId);
            GameObject g = UICanvas.transform.Find(UIName).gameObject;
            UICanvas.SetActive(true);
            g.gameObject.SetActive(true);
            Debug.Log("[UION] Player id : " + playerId);
        }
    }

    public void ClickButton_Chi()
    {
        UIName = "Chi";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }
    public void ClickButton_Pon()
    {
        UIName = "Pon";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }
    public void ClickButton_Kkan()
    {
        UIName = "Kkan";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }
    public void ClickButton_Rich()
    {
        UIName = "Rich";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }
    public void ClickButton_Ron()
    {
        UIName = "Ron";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }
    public void ClickButton_Tsumo()
    {
        UIName = "Tsumo";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }
    public void ClickButton_Skip()
    {
        UIName = "Skip";
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }

    public void _ClickButton()
    {
        findPlayer();
        //여따가 이벤트
        inputEvent.setData(null, UIName);
    }

    public VRCPlayerApi findPlayer()
    {
        return VRCPlayerApi.GetPlayerById(playerId);
    }
}
