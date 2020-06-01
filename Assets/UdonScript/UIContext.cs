
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIContext : UdonSharpBehaviour
{
    // �ִ� �� ������ Chiable ���ٵ� UdonSync�� �������� �� ����ۿ� ����
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
