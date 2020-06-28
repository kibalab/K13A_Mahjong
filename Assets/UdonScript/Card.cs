using UnityEngine;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using VRC.SDKBase;
using UnityEngine.SocialPlatforms;

public class Card : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int GlobalOrder;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;
    [UdonSynced(UdonSyncMode.None)] public Vector3 Position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion Rotation;
    [UdonSynced(UdonSyncMode.None)] public bool IsColliderActive;

    public bool IsRinShan;
    public int YamaIndex;
    public int PlayerIndex;
    public bool IsDiscardedForRiichi;

    [SerializeField] private HandUtil HandUtil;
    [SerializeField] private CardSprites CardSprites;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private BoxCollider BoxColider;
    [SerializeField] private LogViewer LogViewer;
    [SerializeField] private EventQueue EventQueue;

    private bool isSpriteInitialized = false;
    private bool isDoraSetted = false;

    public override void Interact()
    {
        RequestCallFunctionToOwner(nameof(_Interact));
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

    public void Initialize_Master(string type, int cardNumber, bool isDora)
    {
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;
        GlobalOrder = HandUtil.GetGlobalOrder(type, cardNumber);
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
        if (!isSpriteInitialized)
        {
            TryInitializeSprite();
        }

        if (BoxColider != null)
        {
            BoxColider.enabled = IsColliderActive;
        }

        transform.SetPositionAndRotation(Position, Rotation);
    }

    void TryInitializeSprite()
    {
        if (SpriteRenderer != null)
        {
            var spriteName = GetCardSpriteName();
            var sprite = CardSprites.FindSprite(spriteName);
            //var material = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
            

            if (sprite != null)
            {
                SpriteRenderer.sprite = sprite;
                isSpriteInitialized = true;
            }

            if (IsDora && !isDoraSetted)
            {
                setDora();
            }
        }
    }

    public void setDora()
    {
        transform.Find("Display").GetComponentInChildren<Renderer>().material = CardSprites.doraMaterial;
        isDoraSetted = true;
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

    public override string ToString()
    {
        return $"({Type}, {CardNumber}, {GlobalOrder})";
    }
}
