
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class JoinButton : UdonSharpBehaviour
{
    [SerializeField] public EventQueue EventQueue;

    [SerializeField] public LogViewer LogViewer;

    [SerializeField] public JoinStatus JoinManager;

    [SerializeField] public GameObject gameManager;

    public override void Interact()
    {
        Networking.SetOwner(Networking.LocalPlayer, gameManager);

        RequestSerialization();

        JoinManager.Join();
    }
}



//EventQueue.SetRegisterPlayerEvent(newPlayers);