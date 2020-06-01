
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class UIManager : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public int playerId = -1;
    [UdonSynced(UdonSyncMode.None)] public string UIName;
    [UdonSynced(UdonSyncMode.None)] public int playerTurn;

    public GameObject UICanvas;
    public Button Pon, Chi, Kkan, Rich, Ron, Tsumo, Skip;

    private EventQueue eventQueue;

    public void Initialize(EventQueue eq)
    {
        eventQueue = eq;

        UIButton[] uiButton = UICanvas.GetComponentsInChildren<UIButton>(); 


        foreach(UIButton b in uiButton)
        {
            b.Initialize(this);
        }


        if (Networking.LocalPlayer != null)
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "_DisableButton");
        }
        else
        {
            _DisableButton();
        }
        
    }

    public void ActiveButton(string _UIName, VRCPlayerApi player, int turn)
    {
        //Debug.Log("[UION] Player id : " + playerId);
        playerTurn = turn;
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
        if (Networking.LocalPlayer != null)//유니티에서 테스트를 위한 조건문
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
            //Debug.Log("[UION] Player id : " + playerId);
            GameObject g = UICanvas.transform.Find(UIName).gameObject;
            UICanvas.SetActive(true);
            g.gameObject.SetActive(true);
            //Debug.Log("[UION] Player id : " + playerId);
        }
    }

    public void _DisableButton()
    {
        if (Networking.LocalPlayer != null)//유니티에서 테스트를 위한 조건문
        {
            for (var i = 0; i < UICanvas.transform.childCount; i++)
            {
                UICanvas.transform.GetChild(i).gameObject.SetActive(false);
            }
            UICanvas.SetActive(false);
        }
        else
        {
            for (var i = 0; i < UICanvas.transform.childCount; i++)
            {
                UICanvas.transform.GetChild(i).gameObject.SetActive(false);
            }
            UICanvas.SetActive(false);
        }
    }

    public void ClickButton_Chi()
    {
        _ButtonEventInterface("Chi");
    }
    public void ClickButton_Pon()
    {
        _ButtonEventInterface("Pon");

    }
    public void ClickButton_Kkan()
    {
        _ButtonEventInterface("Kkan");
    }
    public void ClickButton_Rich()
    {
        _ButtonEventInterface("Rich");
    }
    public void ClickButton_Ron()
    {
        _ButtonEventInterface("Ron");
    }
    public void ClickButton_Tsumo()
    {
        _ButtonEventInterface("Tsumo");
    }
    public void ClickButton_Skip()
    {
        _ButtonEventInterface("Skip");
    }

    public void _ButtonEventInterface(string uiName)
    {
        UIName = uiName;
        if (Networking.LocalPlayer != null)//유니티에서 테스트를 위한 조건문
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
        } else
        {
            _ClickButton();
        }
        _DisableButton();
    }

    public void _ClickButton()
    {
        Debug.Log("[UION] ClickEvent PlayerTurn : " + playerTurn + ", UIName : " + UIName);
        //findPlayer();
        //여따가 이벤트
        //inputEvent.setData(null, UIName, playerTurn);
        //eventQueue.Enqueue(inputEvent);
    }

    public VRCPlayerApi findPlayer()
    {
        return VRCPlayerApi.GetPlayerById(playerId);
    }
}
