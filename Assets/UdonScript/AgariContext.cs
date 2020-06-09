
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AgariContext : UdonSharpBehaviour
{
    public int[] AgariableCardGlobalOrders;
    public int AgariableCount;

    public bool IsRiichiable;
    public Card[] RiichiCreationCards;

    public bool IsSingleWaiting;

    public bool IsAgariable(Card card)
    {
        return IsAgariableGlobalOrder(card.GlobalOrder);
    }

    public void AddAgariableGlobalOrder(int globalOrder)
    {
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

        IsRiichiable = false;
        IsSingleWaiting = false;
        RiichiCreationCards = null;
    }
}