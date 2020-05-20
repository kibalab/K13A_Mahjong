using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardComponent : UdonSharpBehaviour
{
    public string Type;
    public int CardNumber;
    public int NormalCardNumber;
    public bool IsDora;

    public VRCPlayerApi Owner;

    public GameManager gameManager;
    public EventQueue events;

    public override void Interact()
    {
        
        InteractEventQueue(this);
    }

    public void InteractEventQueue(CardComponent card) // CardComponent().Interect()
    {
        //누가 카드를 클릭했는지 확인하기 위함 
        // (UdonBehaviour 컴포넌으에 "Allow Ownership Transfer on Collision" 체크해줘야함
        // (CardComponent).Owner
        Owner = Networking.GetOwner(this.gameObject);

        events.Enqueue(card);
    }

    public void Initialize(string type, int cardNumber, bool isDora, EventQueue e)
    {
        events = e;
        gameManager = e.gameManager;
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;
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
