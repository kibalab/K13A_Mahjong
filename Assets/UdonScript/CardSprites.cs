
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardSprites : UdonSharpBehaviour
{
    public Sprite FindSprite(string spriteName)
    {
        var spriteGameObject = transform.Find(spriteName);
        var spriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();

        return spriteRenderer.sprite;
    }
}
