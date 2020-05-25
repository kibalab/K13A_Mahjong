
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
        // EventQueue�����ͼ� Ŭ�������� �̺�Ʈ�� �ѱ���ߴµ�
        // �μ��� ��ȯ�� CardComponent�� ���� ī��������Ʈ ����⵵ �� �׷��� ����
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
        //������ �̺�Ʈ
    }

    public VRCPlayerApi findPlayer()
    {
        return VRCPlayerApi.GetPlayerById(playerId);
    }
}
