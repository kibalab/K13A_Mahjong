
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MenuUI : UdonSharpBehaviour
{
    public VRCPlayerApi[] joinPlayer = new VRCPlayerApi[4];
    public int joinPlayerCount = 0;

    private int lastMessageNumber = -1; // 모든 유저용
    [SerializeField] private JoinButton JoinButton;
    [SerializeField] private EventQueue EventQueue;
    private void Update()
    {
        var NetworkMessage = JoinButton.NetworkMessage;

        if (string.IsNullOrEmpty(NetworkMessage))
        {
            return;
        }

        var splited = NetworkMessage.Split(',');
        var networkMessageNumber = int.Parse(splited[0]);

        if (lastMessageNumber != networkMessageNumber)
        {
            lastMessageNumber = networkMessageNumber;

            joinPlayer[joinPlayerCount++] = VRCPlayerApi.GetPlayerById(int.Parse(splited[1]));
            sendEvent();
        }

    }

     void sendEvent()
    {
        EventQueue.SetRegistEvent(joinPlayer[joinPlayerCount - 1]);
    }
}
