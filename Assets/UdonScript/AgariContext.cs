using UdonSharp;

public class AgariContext : UdonSharpBehaviour
{
    public int[] AgariableCardGlobalOrders;
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