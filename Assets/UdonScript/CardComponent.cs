using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardComponent : UdonSharpBehaviour
{
    public string Type;
    public int CardNumber;
    public int normalCardNumber;
    public bool IsDora;
    
    public void Initialize(string type, int cardNumber, bool isDora)
    {
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
