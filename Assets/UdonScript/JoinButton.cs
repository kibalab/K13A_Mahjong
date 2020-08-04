
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class JoinButton : UdonSharpBehaviour
{
    [SerializeField] public EventQueue EventQueue;

    private VRCPlayerApi[] registeredPlayers = null;
    private int registeredPlayerCount;

    public override void Interact()
    {
        // 이건 아무나 부르는 함수
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    // 위의 함수에 의해 밑엣 함수가 모든 유저에게서 호출될거임
    public override void OnOwnershipTransferred()
    {
        // 별도로 초기화 할 곳이 없으니 여기서 조건 체크하고 함
        if (registeredPlayers == null)
        {
            registeredPlayers = new VRCPlayerApi[4];
            registeredPlayerCount = 0;
        }

        var owner = Networking.GetOwner(gameObject);

        // 중복 입장 방지
        if (!IsAlreadyRegistered(owner) && registeredPlayerCount < 4)
        {
            registeredPlayers[registeredPlayerCount++] = owner;
            EventQueue.SetRegisterPlayerEvent(owner);
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
