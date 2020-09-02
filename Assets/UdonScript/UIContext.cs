
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIContext : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public int ChiableCount;
    [UdonSynced(UdonSyncMode.None)] public Vector2 ChiableIndex1;
    [UdonSynced(UdonSyncMode.None)] public Vector2 ChiableIndex2;
    [UdonSynced(UdonSyncMode.None)] public Vector2 ChiableIndex3;

    [UdonSynced(UdonSyncMode.None)] public string ChiableSprite11;
    [UdonSynced(UdonSyncMode.None)] public string ChiableSprite12;
    [UdonSynced(UdonSyncMode.None)] public string ChiableSprite21;
    [UdonSynced(UdonSyncMode.None)] public string ChiableSprite22;
    [UdonSynced(UdonSyncMode.None)] public string ChiableSprite31;
    [UdonSynced(UdonSyncMode.None)] public string ChiableSprite32;

    [UdonSynced(UdonSyncMode.None)] public string AgariableCards;
    [UdonSynced(UdonSyncMode.None)] public int AgariableMessageIndex;

    [UdonSynced(UdonSyncMode.None)] public bool IsTsumoable;
    [UdonSynced(UdonSyncMode.None)] public bool IsRonable;
    [UdonSynced(UdonSyncMode.None)] public bool IsRiichable;
    [UdonSynced(UdonSyncMode.None)] public bool IsChiable;
    [UdonSynced(UdonSyncMode.None)] public bool IsPonable;
    [UdonSynced(UdonSyncMode.None)] public bool IsKkanable;

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
