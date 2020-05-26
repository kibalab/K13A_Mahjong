
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIManager : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public int playerId = -1;
    [UdonSynced(UdonSyncMode.None)] public string UIName;

    public GameObject UICanvas;

    public void Initialize()
    {
        // EventQueue�����ͼ� Ŭ�������� �̺�Ʈ�� �ѱ���ߴµ�
        // �μ��� ��ȯ�� CardComponent�� ���� ī��������Ʈ ����⵵ �� �׷��� ����
    }

    public void ActiveUI(string _UIName, VRCPlayerApi player)
    {
        Debug.Log("[UION] Player id : " + playerId);
        UIName = _UIName;
        if (player != null)
        {
            playerId = player.playerId;
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "_toogleUI");
        } else
        {
            playerId = -1;
            _toogleUI();
        }
    }

    public void _toogleUI()
    {
        if (playerId != -1)//����Ƽ���� �׽�Ʈ�� ���� ���ǹ�
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

    public void ClickButton()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, "_ClickButton");
    }

    public void _ClickButton()
    {
        findPlayer();
        //������ �̺�Ʈ
    }

    public VRCPlayerApi findPlayer()
    {
        return VRCPlayerApi.GetPlayerById(playerId);
    }
}
