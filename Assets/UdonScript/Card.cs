using UnityEngine;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using VRC.SDKBase;
using UnityEngine.SocialPlatforms;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Card : UdonSharpBehaviour
{

    public string Type = "None";
    public int CardNumber = 99;
    public string SpriteNamePostfix = "None";

    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(Network_Position))] public Vector3 Position;
    [UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(Network_Rotation))] public Quaternion Rotation;
    public bool IsColliderActive;
    public bool IsDora;

    public bool IsRinShan;
    public int YamaIndex;
    public int PlayerIndex;
    public bool IsDiscardedForRiichi;
    public int GlobalOrder;

    [SerializeField] private HandUtil HandUtil;
    [SerializeField] private CardSprites CardSprites;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private BoxCollider BoxColider;
    [SerializeField] private LogViewer LogViewer;
    [SerializeField] private EventQueue EventQueue;
    [SerializeField] private AudioQueue AudioQueue;
    [SerializeField] private EventLog EventLog;

    private bool isSpriteInitialized = false;
    private bool isDoraMaterialSetted = false;

    public Vector3 Network_Position
    {
        set
        {
            Position = value;

            transform.position = Position;

            RequestSerialization();
        }
    }
    public Quaternion Network_Rotation
    {
        set
        {
            Rotation = value;

            transform.rotation = Rotation;

            RequestSerialization();
        }
    }

    public override void Interact()
    {
        LogViewer.Log($"Call Interact() from {ToString()}", 0);
        RequestCallFunctionToAll(nameof(l_Interact));
        RequestCallFunctionToAll(nameof(l_playTabSound));
    }


    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        if (player.isMaster)
        {
            Networking.SetOwner(player, gameObject);
        }
    }

    public void resetCard()
    {
        Type = "None";
        CardNumber = -1;
        SpriteNamePostfix = "None";
        IsColliderActive = true;
        IsDora = false;
        Position = new Vector3(0, 0, 0);
        Rotation = new Quaternion(0, 0, 0, 0);
        IsRinShan = false;
        YamaIndex = -1;
        PlayerIndex = -1;
        IsDiscardedForRiichi = false;
        GlobalOrder = -1;
        isSpriteInitialized = false;
        SpriteRenderer.sprite = null;
        isDoraMaterialSetted = false;
    }

    public void refreshSprite()
    {
        InitializeSprite();
    }

    public void l_playTabSound()
    {
        if (AudioQueue != null)
        {
            AudioQueue.AddQueue("CardTabSound");
        }
    }

    public void l_Interact()
    {
        if (IsDiscardedForRiichi)
        {
            EventQueue.SetRiichiEvent(YamaIndex, PlayerIndex);
            IsDiscardedForRiichi = false;
        }
        else
        {
            EventQueue.SetDiscardEvent(YamaIndex, PlayerIndex);
        }
        EventLog.SetEvent($"C&{gameObject.name.Replace("Card (", "").Replace(")", "")}");
    }

    public void Initialize_Master(string type, int cardNumber, bool isRedDora)
    {
        Type = type;
        CardNumber = cardNumber;
        IsDora = isRedDora;
        GlobalOrder = HandUtil.GetGlobalOrder(type, cardNumber);
        SpriteNamePostfix = isRedDora ? "도라" : "";
    }

    public void SetAsDora(bool b)
    {
        IsDora = b;
    }

    public void SetOwnership(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }

    public void SetColliderActivate(bool t)
    {
        IsColliderActive = t;
        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.materials[1].color = t ? Color.white : new Color(0.9f, 0.9f, 0.9f);
    }

    public void SetPosition(Vector3 position, Quaternion rotation, bool synced)
    {
        Network_Position = position;
        Network_Rotation = rotation;
        //transform.SetPositionAndRotation(Position, Rotation);
    }

    public string GetCardSpriteName()
    {
        switch (Type)
        {
            case "만":
            case "삭":
            case "통":
                return Type + CardNumber + SpriteNamePostfix;

            default: // 동서남북백발중
                return Type;
        }
    }

    private void Update()
    {

        /*if (!Networking.IsNetworkSettled)
        {
            return;
        }*/

        if (!isSpriteInitialized && IsCardSpriteInitializeReady())
        {
            InitializeSprite();
        }

        if (BoxColider != null)
        {
            BoxColider.enabled = IsColliderActive;
        }

        if (SpriteRenderer != null && IsDora && !isDoraMaterialSetted)
        {
            SpriteRenderer.material = CardSprites.doraMaterial;
            isDoraMaterialSetted = true;
        }
        else if(!IsDora)
        {
            SpriteRenderer.material = CardSprites.normalMaterial;
            isDoraMaterialSetted = false;
        }
        

        transform.SetPositionAndRotation(Position, Rotation);
    }

    private void Start()
    {
        transform.SetPositionAndRotation(Position, Rotation);
    }

    void InitializeSprite()
    {
        var spriteName = GetCardSpriteName();
        var sprite = CardSprites.FindSprite(spriteName);
        SpriteRenderer.sprite = sprite;
        isSpriteInitialized = true;
    }

    void RequestCallFunctionToOwner(string funcName)
    {
        if (Networking.LocalPlayer == null)
        {
            SendCustomEvent(funcName);
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, funcName);
        }
    }

    void RequestCallFunctionToAll(string funcName)
    {
        if (Networking.LocalPlayer == null)
        {
            SendCustomEvent(funcName);
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, funcName);
        }
    }

    public override string ToString()
    {
        return $"({Type}, {CardNumber}, {GlobalOrder})";
    }

    bool IsCardSpriteInitializeReady()
    {
        return SpriteRenderer != null
            && Type != "None"
            && CardNumber != -1
            && SpriteNamePostfix != "None";
    }
}
