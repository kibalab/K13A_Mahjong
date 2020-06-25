using UnityEngine;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using VRC.SDKBase;
using UnityEngine.SocialPlatforms;

public class Card : UdonSharpBehaviour
{
    private const float ESTIMATED_MAX_NETWORK_DELAY = 3.0f;

    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int GlobalOrder;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;
    [UdonSynced(UdonSyncMode.None)] public bool IsRinShan;

    [UdonSynced(UdonSyncMode.None)] public Vector3 Position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion Rotation;
    [UdonSynced(UdonSyncMode.None)] public int YamaIndex;
    [UdonSynced(UdonSyncMode.None)] public int PlayerIndex;

    [UdonSynced(UdonSyncMode.None)] public float SyncSpriteEndTime = float.MinValue;
    [UdonSynced(UdonSyncMode.None)] public float SyncPositionEndTime = float.MinValue;

    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public CardSprites CardSprites;
    [SerializeField] public SpriteRenderer SpriteRenderer;
    [SerializeField] public BoxCollider BoxColider;
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public CardSyncQueue SyncQueue;

    private InputEvent inputEvent;
    private bool isPrevFrameSpriteSynced;
    private bool isPrevFramePositionSynced;

    Vector3 localPosition;
    Quaternion localRotaiton;


    public override void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            _Interact();
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "_Interact");
        }
    }

    public override void OnOwnershipTransferred()
    {
        LogViewer.Log("Owner Changed. " + Networking.GetOwner(gameObject).displayName , 1);
    }

    public void _Interact()
    {
        inputEvent.SetDiscardEvent(YamaIndex, "Discard", PlayerIndex);
        EventQueue.Enqueue(inputEvent);
    }

    public void Initialize_Master(string type, int cardNumber, bool isDora)
    {
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;
        GlobalOrder = HandUtil.GetGlobalOrder(type, cardNumber);

        SyncSpriteEndTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
        SyncPositionEndTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
    }

    public void SyncData()
    {
        Type = Type;
        CardNumber = CardNumber;
        IsDora = IsDora;
        Position = Position;
        Rotation = Rotation;
        GlobalOrder = GlobalOrder;
        YamaIndex = YamaIndex;

        SyncSpriteEndTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
        SyncPositionEndTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
    }

    public void SetOwnership(int playerIndex, InputEvent inputEvent)
    {
        this.inputEvent = inputEvent;
        this.PlayerIndex = playerIndex;
    }

    public void SetColliderActivate(bool t)
    {
        BoxColider.enabled = t;
    }

    public void SyncPosition()
    {
        Position = localPosition;
        Rotation = localRotaiton;

        SyncPositionEndTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
    }

    public void SetPosition(Vector3 p, Quaternion r)
    {
        localPosition = p;
        localRotaiton = r;

        SyncQueue.AddSync(YamaIndex);
    }

    public string GetCardSpriteName()
    {
        switch (Type)
        {
            case "동":
            case "남":
            case "서":
            case "북":
            case "백":
            case "발":
            case "중":
                return Type;
            default:
                return Type + CardNumber + (IsDora ? "도라" : "");
        }
    }

    private void Update()
    {
        var now = Time.time;

        var onSyncSprite = now < SyncSpriteEndTime;
        if (onSyncSprite && !isPrevFrameSpriteSynced)
        {
            SyncSprite_Client();
        }

        isPrevFrameSpriteSynced = onSyncSprite;

        var onSyncPosition = now < SyncPositionEndTime;
        if (onSyncPosition && !isPrevFramePositionSynced)
        {
            SyncPosition_Client();
        }

        isPrevFramePositionSynced = onSyncPosition;
    }

    void SyncSprite_Client()
    {
        var spriteName = GetCardSpriteName();
        var sprite = CardSprites.FindSprite(spriteName);
        SpriteRenderer.sprite = sprite;
        LogViewer.Log($"Sprite Synced. ({YamaIndex}: {Type}, {CardNumber}, {GlobalOrder})", 1);
    }

    void SyncPosition_Client()
    {
        transform.SetPositionAndRotation(Position, Rotation);
        LogViewer.Log($"Position Synced. ({Type}, {CardNumber}, {GlobalOrder})", 1);
    }
}
