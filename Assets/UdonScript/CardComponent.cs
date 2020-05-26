using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class CardComponent : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int NormalCardNumber;
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

    public void Initialize(string type, int cardNumber, bool isDora, EventQueue e, CardSprites sprites)
    {
        eventQueue = e;
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;

        NormalCardNumber = GetGlobalOrder();
        collider = this.GetComponent<BoxCollider>();

        //uiManager = this.gameObject.GetComponentInChildren<UIManager>();

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

    int GetGlobalOrder()
    {
        var typeOrder = 100;
        switch (Type)
        {
            case "만": typeOrder *= 0; break;
            case "삭": typeOrder *= 1; break;
            case "통": typeOrder *= 2; break;
            case "동": typeOrder *= 3; break;
            case "남": typeOrder *= 4; break;
            case "서": typeOrder *= 5; break;
            case "북": typeOrder *= 6; break;
            case "백": typeOrder *= 7; break;
            case "발": typeOrder *= 8; break;
            case "중": typeOrder *= 9; break;
        }
        var cardOrder = CardNumber * 10;
        var doraOrder = IsDora ? 1 : 0;

        return typeOrder + cardOrder + doraOrder;
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
