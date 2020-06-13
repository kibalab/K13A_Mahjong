using UnityEngine;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using VRC.SDKBase;

public class Card : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int GlobalOrder;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;
    [UdonSynced(UdonSyncMode.None)] public bool IsRinShan;

    [UdonSynced(UdonSyncMode.None)] public Vector3 position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion rotation;
    [UdonSynced(UdonSyncMode.None)] public int YamaIndex;
    [UdonSynced(UdonSyncMode.None)] public int PlayerIndex;

    [UdonSynced(UdonSyncMode.None)] public bool IsChanged;
    [UdonSynced(UdonSyncMode.None)] public bool IsPositionChanged;

    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public CardSprites CardSprites;
    [SerializeField] public SpriteRenderer SpriteRenderer;
    [SerializeField] public BoxCollider BoxColider;
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public EventQueue EventQueue;

    private InputEvent inputEvent;

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

        IsChanged = true;
    }

    public void SyncData()
    {
        Type = Type;
        CardNumber = CardNumber;
        IsDora = IsDora;
        IsRinShan = IsRinShan;
        position = position;
        rotation = rotation;

        IsChanged = true;
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
        IsPositionChanged = true;
    }

    void Update()
    {
        if (IsChanged)
        {
            IsChanged = false;

            transform.SetPositionAndRotation(position, rotation);
            var spriteName = GetCardSpriteName();
            var sprite = CardSprites.FindSprite(spriteName);
            SpriteRenderer.sprite = sprite;
        }

        if (IsPositionChanged)
        {
            IsPositionChanged = false;

            transform.SetPositionAndRotation(position, rotation);
        }
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
}
