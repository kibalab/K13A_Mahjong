
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardSprites : UdonSharpBehaviour
{
    public Sprite FindSprite(string spriteName)
    {
        var spriteGameObject = transform.Find(spriteName);
        if (spriteGameObject == null) { return null; }

        var spriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) { return null; }
        
        return spriteRenderer.sprite;
    }
}
