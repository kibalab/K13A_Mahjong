
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class JoinStatus : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string NetworkMessage;

    // 마스터 전용
    private int messageNumber = 0;

    // 모든 유저용
    private int lastMessageNumber = -1;

    public Text server;
    public Text joinList;
    public Text joinCount;
    public Image[] joinIcons;

    public GameObject gameManager;

    public LogViewer LogViewer;

    private void Start()
    {
        //setNetworkMessage("null [Test],null[Test]");
        resetJoinIcon();
        joinCount.text = "Wait for Join";
    }

    public void setNetworkMessage(string data)
    {
        NetworkMessage = SerializeJoinList(data);
    }

    public string SerializeJoinList(string list)
    {
        var serializedString = $"{messageNumber++},{list}";
        return serializedString;
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(NetworkMessage))
        {
            return;
        }
        if (Networking.LocalPlayer != null)
        {
            var owner = Networking.GetOwner(gameManager);
            server.text = $"Instance Server : {owner.displayName}";
        }

        

        var splited = NetworkMessage.Split(',');
        var networkMessageNumber = int.Parse(splited[0]);

        if (lastMessageNumber != networkMessageNumber)
        {

            
            lastMessageNumber = networkMessageNumber;

            resetJoinIcon();
            joinList.text = "";
            for(int i=1, k=0; i < splited.Length; i++)
            {
                LogViewer.Log($"[JoinStatus] ListCount : {i}", 1);
                joinList.text += $"{splited[i]}\n";  
                joinIcons[k++].color = Color.white;
                joinCount.text = (4 - i) > 0 ? $"{4 - i} Player Left" : "Player is All Ready";
            }
        }
    }

    void resetJoinIcon()
    {
        foreach(Image icon in joinIcons)
        {
            icon.color = new Color(0,0,0, 0.39f);
        }
    }

} 
