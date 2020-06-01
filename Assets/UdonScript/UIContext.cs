
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIContext : UdonSharpBehaviour
{
    // 최대 두 개까지 Chiable 할텐데 UdonSync로 보낼려면 이 방법밖에 없음
    [UdonSynced(UdonSyncMode.None)] public int ChiableIndex1;
    [UdonSynced(UdonSyncMode.None)] public int ChiableIndex2;
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
}
