using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardSprites : UdonSharpBehaviour
{
    [SerializeField] public Material doraMaterial;
    public Sprite FindSprite(string spriteName)
    {
        var spriteGameObject = transform.Find(spriteName);
        if (spriteGameObject == null)
        {
            return null;
        }

        var spriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();
        return spriteRenderer.sprite;
    }
}
