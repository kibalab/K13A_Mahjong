
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class JoinButton : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string NetworkMessage = "";

    private int messageNumber = 0; // 마스터 전용
    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }
    string SerializePlayer(string PlayerID)
    {
        var serializedString = $"{messageNumber++},{PlayerID}";
        return serializedString;
    }
    public override void OnOwnershipTransferred()
    {
        NetworkMessage = SerializePlayer(Networking.LocalPlayer.playerId.ToString());
    }
}
