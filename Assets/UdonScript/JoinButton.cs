
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class JoinButton : UdonSharpBehaviour
{
    [SerializeField] public EventQueue EventQueue;

    [SerializeField] public LogViewer LogViewer;

    [SerializeField] public GameObject gameManager;

    private VRCPlayerApi[] registeredPlayers = null;
    private int registeredPlayerCount;

    public bool testMode = false;

    public override void Interact()
    {
        // �̰� �ƹ��� �θ��� �Լ�
        Networking.SetOwner(Networking.LocalPlayer, gameObject); 
        LogViewer.Log($"[JoinButton] SetOwner : {Networking.GetOwner(gameObject)}", 1);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Registering));
    }

    // ���� �Լ��� ���� �ؿ� �Լ��� ��� �������Լ� ȣ��ɰ���
    /*
    public override void OnOwnershipTransferred() 
    {
        LogViewer.Log("[JoinButton] OwnershipTransferred", 0);
        Registering();
    }*/

    public void Registering()
    {
        
        LogViewer.Log($"[JoinButton] Request Registering Player : {Networking.GetOwner(gameObject).displayName}", 0);
        if (Networking.GetOwner(gameManager) != Networking.LocalPlayer)
        {
            return;
        }
        
        // ������ �ʱ�ȭ �� ���� ������ ���⼭ ���� üũ�ϰ� ��
        if (registeredPlayers == null)
        {
            registeredPlayers = new VRCPlayerApi[4];
            registeredPlayerCount = 0;
        }

        var owner = Networking.GetOwner(gameObject);

        

        // �ߺ� ���� ����
        if (!IsAlreadyRegistered(owner) && registeredPlayerCount < 4 || testMode)
        {
            registeredPlayers[registeredPlayerCount++] = owner;
            EventQueue.SetRegisterPlayerEvent(owner);
        }
        else
        {
            LogViewer.Log($"[JoinButton] {owner.displayName} is Already Registered Player", 0);
        }
    }

    bool IsAlreadyRegistered(VRCPlayerApi player)
    {
        for(var i = 0; i < registeredPlayerCount; ++i)
        {
            if (registeredPlayers[i].displayName == player.displayName)
            {
                return true;
            }
        }
        return false;
    }

}
