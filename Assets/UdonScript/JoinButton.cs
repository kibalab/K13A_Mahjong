
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
        // �̰� �ƹ��� �θ��� �Լ�
        Networking.SetOwner(Networking.LocalPlayer, gameObject);
    }

    // ���� �Լ��� ���� �ؿ� �Լ��� ��� �������Լ� ȣ��ɰ���
    public override void OnOwnershipTransferred()
    {
        // ������ �ʱ�ȭ �� ���� ������ ���⼭ ���� üũ�ϰ� ��
        if (registeredPlayers == null)
        {
            registeredPlayers = new VRCPlayerApi[4];
            registeredPlayerCount = 0;
        }

        var owner = Networking.GetOwner(gameObject);

        // �ߺ� ���� ����
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
