using UdonSharp;
using UnityEngine; 
using VRC.SDKBase;
using VRC.Udon;

public class CardComponent : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int NormalCardNumber;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;
    [UdonSynced(UdonSyncMode.None)] public string EventType;

    private VRCPlayerApi owner;
    private EventQueue eventQueue;
    private BoxCollider collider;

    public override void Interact()
    {
        //누가 카드를 클릭했는지 확인하기 위함 
        // (UdonBehaviour 컴포넌트에 "Allow Ownership Transfer on Collision" 체크해줘야함
        // (CardComponent).Owner
        owner = Networking.GetOwner(this.gameObject);

        EventType = "Discard";

        eventQueue.Enqueue(this);
    }

    public BoxCollider SetColliderActivate(bool t)
    {
        collider.enabled = t;
        return collider;
    }

    public void Initialize(string type, int cardNumber, bool isDora, EventQueue e)
    {
        eventQueue = e;
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;

        NormalCardNumber = GetGlobalOrder();
        collider = this.GetComponent<BoxCollider>();
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

    public void SetPosition(Vector3 position, Quaternion rotation)
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
}
