using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class CardComponent : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int GlobalIndex;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;

    [UdonSynced(UdonSyncMode.None)] public Vector3 position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion rotation;

    private VRCPlayerApi owner;
    private EventQueue eventQueue;
    public InputActionEvent InputEvent;
    private BoxCollider collider;

    public UIManager uiManager;

    public Text DebugText;

    public override void Interact()
    {
        InputEvent.setData(this, "Discard", -1);
        eventQueue.Enqueue(InputEvent);
    }

    public BoxCollider SetColliderActivate(bool t)
    {
        collider.enabled = t;
        return collider;
    }

    public void Initialize(string type, int cardNumber, bool isDora, EventQueue e, CardSprites sprites, HandUtil util)
    {
        eventQueue = e;
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;

        GlobalIndex = util.CardComponentToIndex(type, cardNumber);
        collider = this.GetComponent<BoxCollider>();

        var spriteName = GetCardSpriteName();
        var sprite = sprites.FindSprite(spriteName);
        SetSprite(sprite);
    }

    public void SetParent(GameObject gameObject)
    {
        transform.SetParent(gameObject.transform);
    }

    public void SetSprite(Sprite sprite)
    {
        var display = transform.Find("Display");
        var renderer = display.GetComponent<SpriteRenderer>();

        renderer.sprite = sprite;
    }

    public void SetPosition(Vector3 p, Quaternion r)
    {
        position = p;
        rotation = r;
        //SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "_SetPosition");
        _SetPosition();
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
