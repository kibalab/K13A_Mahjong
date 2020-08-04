
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
        // 이건 아무나 부르는 함수
        Networking.SetOwner(Networking.LocalPlayer, gameObject); 
        LogViewer.Log($"[JoinButton] SetOwner : {Networking.GetOwner(gameObject)}", 1);
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(Registering));
    }

    // 위의 함수에 의해 밑엣 함수가 모든 유저에게서 호출될거임
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
        
        // 별도로 초기화 할 곳이 없으니 여기서 조건 체크하고 함
        if (registeredPlayers == null)
        {
            registeredPlayers = new VRCPlayerApi[4];
            registeredPlayerCount = 0;
        }

        var owner = Networking.GetOwner(gameObject);

        

        // 중복 입장 방지
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
