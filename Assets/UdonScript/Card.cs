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

    public UIManager UIManager;

    private InputEvent inputEvent;
    private EventQueue eventQueue;
    private BoxCollider boxCollider;

    public LogViewer LogViewer;

    // 월드 마스터의 local에서만 true인 항목
    private bool isRunOnMasterScript = false;

    public override void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            _Interact();
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "_Interact");
        }
    }

    public void _Interact()
    {
        if (isRunOnMasterScript)
        {
            inputEvent.SetDiscardEvent(YamaIndex, "Discard", PlayerIndex);
            eventQueue.Enqueue(inputEvent);
        }
    }

    public void Initialize_Master(string type, int cardNumber, bool isDora, bool isRinShan)
    {
        LogViewer.Log($"Set Owner (TableManager, {Networking.LocalPlayer.displayName})", 0);

        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;
        IsRinShan = isRinShan;

        // 마스터만 해당 bool값이 true이다
        isRunOnMasterScript = true;
    }

    public void syncData()
    {
        SendCustomNetworkEvent(NetworkEventTarget.Owner, "_syncData");
    }

    public void _syncData()
    {
        Type = Type;
        CardNumber = CardNumber;
        IsDora = IsDora;
        IsRinShan = IsRinShan;
        SetPosition(transform.position, transform.rotation);
    }

    public void Initialize_All(EventQueue eventQueue, HandUtil util, CardSprites sprites, Material material)
    {
        this.eventQueue = eventQueue;
        GlobalOrder = util.GetGlobalOrder(Type, CardNumber);
        boxCollider = this.GetComponent<BoxCollider>();

        var spriteName = GetCardSpriteName();
        var sprite = sprites.FindSprite(spriteName);
        if (GlobalOrder != 0)
        {
            LogViewer.Log($"LocalPlayer Card Initalizing (Name: {Type}{CardNumber}, GlobalOrder: {GlobalOrder})", 1);
            LogViewer.Log($"Card Sprite Name : {spriteName}", 1);
            LogViewer.Log($"Get Card Sprite", 1);
        }
        else
        {
            LogViewer.ErrorLog($"LocalPlayer Card Initalizing Failed", 1);
        }

        SetSprite(sprite);
        setMaterial(material);
    }

    public void SetOwnership(int playerIndex, InputEvent inputEvent)
    {
        this.inputEvent = inputEvent;
        this.PlayerIndex = playerIndex;
    }

    public void SetSprite(Sprite sprite)
    {
        var display = transform.Find("Display");
        var renderer = display.GetComponent<SpriteRenderer>();

        renderer.sprite = sprite;
    }

    public void setMaterial(Material material)
    {
        var display = transform.Find("Display");
        var renderer = display.GetComponent<SpriteRenderer>();
        if (IsDora)
        {
            renderer.material = material;
        }
        else
        {
            renderer.material = material;
        }
    }

    public BoxCollider SetColliderActivate(bool t)
    {
        boxCollider.enabled = t;
        return boxCollider;
    }

    public void SetPosition(Vector3 p, Quaternion r)
    {
        LogViewer.Log($"Set Owner (TableManager, {Networking.LocalPlayer.displayName})", 0);

        position = p;
        rotation = r;

        if (Networking.LocalPlayer == null)
        {
            _SetPosition();
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "_SetPosition");
        }
    }

    public void _SetPosition()
    {
        transform.SetPositionAndRotation(position, rotation);
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
