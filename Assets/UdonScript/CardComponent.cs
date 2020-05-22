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
}
