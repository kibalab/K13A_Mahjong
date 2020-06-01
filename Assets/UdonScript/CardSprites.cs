
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardSprites : UdonSharpBehaviour
{
    public Sprite FindSprite(string spriteName)
    {
        var spriteGameObject = transform.Find(spriteName);
        if (spriteGameObject == null)
        {
            // 가끔 UTF-8 인코딩이 아니라서 못 찾는 경우가 있음.
            // 그냥 터지게 한다
            Debug.Log($"{spriteName}을 찾지 못했습니다.");
        }
        
        var spriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();

        return spriteRenderer.sprite;
    }
}
