
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class JoinStatus : UdonSharpBehaviour
{
    public Text server;
    public Text joinList;
    public Text joinCount;
    public Image[] joinIcons;

    public GameObject gameManager;

    public JoinButton JoinButton;

    public LogViewer LogViewer;

    public KList JoinnedPlayers;

    public EventQueue EventQueue;

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(Net_JoinnedPlayersID))] public int[] JoinnedPlayersID;

    public int[] Net_JoinnedPlayersID
    {
        set
        {
            JoinnedPlayersID = value;

            JoinnedPlayers.Clear();
            foreach (var id in value)
            {
                var player = VRCPlayerApi.GetPlayerById(id);
                JoinnedPlayers.Add(player);
            }

            

            EventQueue.SetRegisterPlayerEvent((VRCPlayerApi[])JoinnedPlayers.Clone());

            UpdateJoinIcon();
        }
    }

    private void Start()
    {
        ResetJoinIcon();
        joinCount.text = "Wait for Join";
    }

    public void Join()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.Owner, nameof(Registering));
    }

    public void Registering()
    {
        var ps = "";
        for (var i = 0; i < JoinnedPlayers.Count(); i++)
        {
            ps += ((VRCPlayerApi)JoinnedPlayers.At(i)).displayName + ", ";
            LogViewer.Log($"[JoinStatus] Current Joineed Players : {ps}", 1);
        }

        var player = Networking.GetOwner(JoinButton.gameManager);

        LogViewer.Log($"[JoinStatus] {player.displayName} is already joinned.", 1);


        for(var i = 0; i< JoinnedPlayers.Count(); i++)
        {
            if (((VRCPlayerApi)JoinnedPlayers.At(i)).Equals(player))
            {
                LogViewer.Log($"[JoinStatus] {player.displayName} is already joinned.", 1);
                UpdateJoinIcon();
                return;
            }
        }

        LogViewer.Log($"[JoinStatus] {player.displayName} has joinned.", 1);
        JoinnedPlayers.Add(player);

        JoinnedPlayersID = GetJoinnedPlayerIDs();
        RequestSerialization();

        EventQueue.SetRegisterPlayerEvent((VRCPlayerApi[])JoinnedPlayers.Clone());

        UpdateJoinIcon();
    }

    public void UpdateJoinIcon()
    {
        ResetJoinIcon();

        joinList.text = "";

        var ps = "";
        for (var i = 0; i < JoinnedPlayers.Count(); i++)
        {
            ps += ((VRCPlayerApi)JoinnedPlayers.At(i)).displayName + "\n";
            joinIcons[i].color = Color.white;
        }
        joinList.text = ps;
        joinCount.text = (4 - JoinnedPlayers.Count()) > 0 ? $"{4 - JoinnedPlayers.Count()} Player Left" : "Player is All Ready";
    }

    private void Update()
    {
        if (Networking.LocalPlayer != null)
        {
            var owner = Networking.GetOwner(gameManager);
            server.text = $"Instance Server : {owner.displayName}";
        }

        UpdateJoinIcon();
    }


    int[] GetJoinnedPlayerIDs()
    {
        int[] ids = new int[JoinnedPlayers.Count()];
        for (var i = 0; i < ids.Length; i++)
        {
            ids[i] = ((VRCPlayerApi)JoinnedPlayers.At(i)).playerId;
        }
        return ids;
    }

    void ResetJoinIcon()
    {
        foreach (Image icon in joinIcons)
        {
            icon.color = new Color(0, 0, 0, 0.39f);
        }
    }
} 
