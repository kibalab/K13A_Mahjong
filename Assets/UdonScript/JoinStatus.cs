
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class JoinStatus : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string NetworkMessage;

    // ������ ����
    private int messageNumber = 0;

    // ��� ������
    private int lastMessageNumber = -1;

    public Text server;
    public Text joinList;
    public Text joinCount;
    public Image[] joinIcons;

    string SerializeJoinData(string list, string text)
    {
        var serializedString = $"{messageNumber++}&{list}&{text}";
        return serializedString;
    }

} 
