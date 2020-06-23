using UnityEngine;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using VRC.SDKBase;

public class Card : UdonSharpBehaviour
{
    private const float ESTIMATED_MAX_NETWORK_DELAY = 3.0f;

    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int GlobalOrder;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;
    [UdonSynced(UdonSyncMode.None)] public bool IsRinShan;

    [UdonSynced(UdonSyncMode.None)] public Vector3 position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion rotation;
    [UdonSynced(UdonSyncMode.None)] public int YamaIndex;
    [UdonSynced(UdonSyncMode.None)] public int PlayerIndex;

    [UdonSynced(UdonSyncMode.None)] public float SyncSpriteStartTime = float.MaxValue;
    [UdonSynced(UdonSyncMode.None)] public float SyncPositionStartTime = float.MaxValue;

    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public CardSprites CardSprites;
    [SerializeField] public SpriteRenderer SpriteRenderer;
    [SerializeField] public BoxCollider BoxColider;
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public EventQueue EventQueue;

    private InputEvent inputEvent;
    private bool isPrevFrameSpriteSynced;
    private bool isPrevFramePositionSynced;


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

        SyncSpriteStartTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
        SyncPositionStartTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
    }

    public void SyncData()
    {
        Type = Type;
        CardNumber = CardNumber;
        IsDora = IsDora;
        position = position;
        rotation = rotation;
        GlobalOrder = GlobalOrder;
        YamaIndex = YamaIndex;

        SyncSpriteStartTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
        SyncPositionStartTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
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

    public void SetPosition(Vector3 p, Quaternion r)
    {
        position = p;
        rotation = r;

        SyncPositionStartTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
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

        var isSyncSprite = SyncSpriteStartTime < now;
        if (isSyncSprite && !isPrevFrameSpriteSynced)
        {
            SyncSprite();
        }

        isPrevFrameSpriteSynced = isSyncSprite;

        var isSyncPosition = SyncPositionStartTime < now;
        if (isSyncPosition && !isPrevFramePositionSynced)
        {
            SyncPosition();
        }

        isPrevFramePositionSynced = isSyncPosition;
    }

    void SyncSprite()
    {
        var spriteName = GetCardSpriteName();
        var sprite = CardSprites.FindSprite(spriteName);
        SpriteRenderer.sprite = sprite;
        LogViewer.Log($"Sprite Synced. ({YamaIndex}: {Type}, {CardNumber}, {GlobalOrder})", 1);
    }

    void SyncPosition()
    {
        transform.SetPositionAndRotation(position, rotation);
        LogViewer.Log($"Position Synced. ({Type}, {CardNumber}, {GlobalOrder})", 1);
    }
}
