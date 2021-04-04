using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardSprites : UdonSharpBehaviour
{
    [SerializeField] public Material doraMaterial;
    [SerializeField] public Material normalMaterial;
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

    public Sprite FindSpriteWithInt(int globalOrder)
    {
        /*var l = 0;
        Transform spriteGameObject = null;
        for (var i =0; l<=globalOrder; i++)
        {
            spriteGameObject = transform.GetChild(i);
            if (!spriteGameObject.name.EndsWith("µµ¶ó"))
            {
                l++;
            }
        }*/

        if (4 < globalOrder)
            globalOrder += 1;
        if (13 < globalOrder)
            globalOrder += 1;
        if (21 < globalOrder)
            globalOrder += 1;

        var spriteGameObject = transform.GetChild(globalOrder);

        var spriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();
        return spriteRenderer.sprite;

        /*var spriteGameObject = transform.GetChild(globalOrder);
        if (spriteGameObject == null)
        {
            return null;
        }

        var spriteRenderer = spriteGameObject.GetComponent<SpriteRenderer>();
        return spriteRenderer.sprite;*/
    }
}
