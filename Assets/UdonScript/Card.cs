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

    [UdonSynced(UdonSyncMode.None)] public float SyncSpriteEndTime = 0f;
    [UdonSynced(UdonSyncMode.None)] public float SyncPositionEndTime = 0f;

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

        SyncSpriteEndTime = Time.time + 5.0f;
        SyncPositionEndTime = Time.time + 5.0f;
    }

    public void SyncData()
    {
        Type = Type;
        CardNumber = CardNumber;
        IsDora = IsDora;
        position = position;
        rotation = rotation;
        GlobalOrder = GlobalOrder;

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

    public void SetPosition(Vector3 p, Quaternion r)
    {
        position = p;
        rotation = r;

        SyncPositionEndTime = Time.time + ESTIMATED_MAX_NETWORK_DELAY;
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

        CheckSpriteSync(now);
        CheckPositionSync(now);
    }

    void CheckSpriteSync(float now)
    {
        var isSyncSprite = now < SyncSpriteEndTime;
        if (isSyncSprite && !isPrevFrameSpriteSynced)
        {
            var spriteName = GetCardSpriteName();
            var sprite = CardSprites.FindSprite(spriteName);
            SpriteRenderer.sprite = sprite;

            LogViewer.Log($"Sprite Synced. ({Type}, {CardNumber}, {GlobalOrder})", 1);
        }

        isPrevFrameSpriteSynced = isSyncSprite;
    }

    void CheckPositionSync(float now)
    {
        var isSyncPosition = now < SyncPositionEndTime;
        if (isSyncPosition && !isPrevFramePositionSynced)
        {
            transform.SetPositionAndRotation(position, rotation);

            LogViewer.Log($"Position Synced. ({Type}, {CardNumber}, {GlobalOrder})", 1);
        }

        isPrevFramePositionSynced = isSyncPosition;
    }

}
