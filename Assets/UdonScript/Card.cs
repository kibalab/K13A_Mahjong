using UnityEngine;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using VRC.SDKBase;
using UnityEngine.SocialPlatforms;

public class Card : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type = "None";
    [UdonSynced(UdonSyncMode.None)] public int CardNumber = -1;
    [UdonSynced(UdonSyncMode.None)] public string SpriteNamePostfix = "None";

    [UdonSynced(UdonSyncMode.None)] public Vector3 Position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion Rotation;
    [UdonSynced(UdonSyncMode.None)] public bool IsColliderActive;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;

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

    private bool isSpriteInitialized = false;
    private bool isDoraMaterialSetted = false;

    public override void Interact()
    {
        RequestCallFunctionToOwner(nameof(_Interact));
        RequestCallFunctionToOwner(nameof(_playTabSound));
    }

    public void _playTabSound()
    {
        if (AudioQueue != null)
        {
            AudioQueue.AddQueue("CardTabSound");
        }
    }

    public void _Interact()
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
    }

    public void Initialize_Master(string type, int cardNumber, bool isRedDora)
    {
        Type = type;
        CardNumber = cardNumber;
        IsDora = isRedDora;
        GlobalOrder = HandUtil.GetGlobalOrder(type, cardNumber);
        SpriteNamePostfix = isRedDora ? "도라" : "";
    }

    public void SetAsDora()
    {
        IsDora = true;
    }

    public void SetOwnership(int playerIndex)
    {
        PlayerIndex = playerIndex;
    }

    public void SetColliderActivate(bool t)
    {
        IsColliderActive = t;
    }

    public void SetPosition(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
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
