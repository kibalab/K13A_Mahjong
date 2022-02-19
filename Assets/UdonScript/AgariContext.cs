using UdonSharp;
using UnityEngine;

public class AgariContext : UdonSharpBehaviour
{
    public int[] AgariableCardGlobalOrders;
    public int[] AgariableCardCounts = new int[34]; // TILE_COUNT => HandUtil
    public int AgariableCount;
    public Card[] RiichiCreationCards;

    public bool IsSingleWaiting;

    public int shantenIndex = 1;

    public int SetShantenCount(int i) => shantenIndex = i;

    public bool IsAgariable(Card card)
    {
        return IsAgariableGlobalOrder(card.GlobalOrder);
    }

    public void AddAgariableGlobalOrder(int globalOrder)
    {
        if (globalOrder < 0 || globalOrder >= 34)
        {
            Debug.Log($"[AgariContext] {globalOrder} is cannot Agariable!");
        }

        if (!IsAgariableGlobalOrder(globalOrder))
        {
            AgariableCardGlobalOrders[AgariableCount++] = globalOrder;
        }
    }

    bool IsAgariableGlobalOrder(int val)
    {
        for (var i = 0; i < AgariableCount; ++i)
        {
            if (AgariableCardGlobalOrders[i] == val) { return true; }
        }
        return false;
    }

    public void Clear()
    {
        AgariableCardGlobalOrders = new int[14];
        AgariableCount = 0;

        IsSingleWaiting = false;
        RiichiCreationCards = null;
    }
}