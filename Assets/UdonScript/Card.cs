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

    [UdonSynced(UdonSyncMode.None)] public bool IsColliderActive;

    [SerializeField] public HandUtil HandUtil;
    [SerializeField] public CardSprites CardSprites;
    [SerializeField] public SpriteRenderer SpriteRenderer;
    [SerializeField] public BoxCollider BoxColider;
    [SerializeField] public LogViewer LogViewer;
    [SerializeField] public EventQueue EventQueue;

    private InputEvent inputEvent;
    private bool isSpriteInitialized = false;

    public override void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            _Interact();
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(_Interact));
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
    }

    public void SetOwnership(int playerIndex, InputEvent inputEvent)
    {
        this.inputEvent = inputEvent;
        this.PlayerIndex = playerIndex;
    }

    public void SetColliderActivate(bool t)
    {
        IsColliderActive = t;
    }

    public void SetPosition(Vector3 p, Quaternion r)
    {
        Position = p;
        Rotation = r;
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

    public bool IsInit()
    {
        return isSpriteInitialized;
    }


    private void Update()
    {
        if (!isSpriteInitialized && SpriteRenderer != null)
        {
            var spriteName = GetCardSpriteName();
            var sprite = CardSprites.FindSprite(spriteName);
            if (sprite != null)
            {
                SpriteRenderer.sprite = sprite;
                isSpriteInitialized = true;
            }
        }

        if (BoxColider != null)
        {
            BoxColider.enabled = IsColliderActive;
        }

        transform.SetPositionAndRotation(Position, Rotation);
    }
}
