
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIContext : UdonSharpBehaviour
{
    public int ChiableCount;
    public Vector2 ChiableIndex1;
    public Vector2 ChiableIndex2;
    public Vector2 ChiableIndex3;

    public string ChiableSprite11;
    public string ChiableSprite12;
    public string ChiableSprite21;
    public string ChiableSprite22;
    public string ChiableSprite31;
    public string ChiableSprite32;

    public string AgariableCards;
    public int AgariableMessageIndex;

    public bool IsTsumoable;
    public bool IsRonable;
    public bool IsRiichable;
    public bool IsChiable;
    public bool IsPonable;
    public bool IsKkanable;


    public bool IsAnythingActived()
    {
        return IsChiable || IsPonable || IsKkanable || IsRiichable || IsRonable || IsTsumoable;
    }

    public void Clear()
    {
        IsTsumoable = false;
        IsRonable = false;
        IsRiichable = false;
        IsChiable = false;
        IsPonable = false;
        IsKkanable = false;
    }

    public void SetAgarible(string[] cards)
    {
        AgariableMessageIndex = (AgariableMessageIndex + 1) / 2;
        var str = "";
        for (var i = 0; i < cards.Length; i++)
        {
            str += cards[i];
            if (i != cards.Length - 1)
            {
                str += ",";
            }
        }
    }

    public void SetChiable(int slot, int[] yamaIndexes, string[] spriteNames)
    {
        switch (slot)
        {
            case 0:
                ChiableIndex1 = new Vector2(yamaIndexes[0], yamaIndexes[1]);
                ChiableSprite11 = spriteNames[0];
                ChiableSprite12 = spriteNames[1];
                break;

            case 1:
                ChiableIndex2 = new Vector2(yamaIndexes[0], yamaIndexes[1]);
                ChiableSprite21 = spriteNames[0];
                ChiableSprite22 = spriteNames[1];
                break;

            case 2:
                ChiableIndex3 = new Vector2(yamaIndexes[0], yamaIndexes[1]);
                ChiableSprite31 = spriteNames[0];
                ChiableSprite32 = spriteNames[1];
                break;
        }
    }

    public string[] GetCardSpriteNames(int slot)
    {
        switch(slot)
        {
            case 0: return new string[] { ChiableSprite11, ChiableSprite12 };
            case 1: return new string[] { ChiableSprite21, ChiableSprite22 };
            case 2: return new string[] { ChiableSprite31, ChiableSprite32 };
        }
        return new string[] { };
    }
}
