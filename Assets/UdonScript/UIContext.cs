
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

    [UdonSynced(UdonSyncMode.None)] public int PonableIndex;
    [UdonSynced(UdonSyncMode.None)] public int KkanableIndex;

    [UdonSynced(UdonSyncMode.None)] public bool IsTsumoable;
    [UdonSynced(UdonSyncMode.None)] public bool IsRonable;
    [UdonSynced(UdonSyncMode.None)] public bool IsRiichable;
    [UdonSynced(UdonSyncMode.None)] public bool IsChiable;
    [UdonSynced(UdonSyncMode.None)] public bool IsPonable;
    [UdonSynced(UdonSyncMode.None)] public bool IsKkanable;

    public bool IsChanged;

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
