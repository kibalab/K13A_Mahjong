
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerDebugUI : UdonSharpBehaviour
{
    public AgariContext agariContext;
    public Text AgariableCardGlobalOrders, AgariableCount, RiichiCreationCards;
    public PlayerStatus playerStatus;
    public Text IsRiichiMode;
    public Player player;
    public Text Cards;



    private void Update()
    {
        AgariableCardGlobalOrders.text = intToString(agariContext.AgariableCardGlobalOrders);
        AgariableCount.text = agariContext.AgariableCount.ToString();
        RiichiCreationCards.text = cardsToString(agariContext.RiichiCreationCards);
        IsRiichiMode.text = playerStatus.IsRiichiMode.ToString();
        Cards.text = "";
        for(var i = 0; i < player.Cards.Count(); i++)
        {
            Cards.text += ((Card)player.Cards.At(i)).ToString() + ", ";
        }
    }

    string cardsToString(Card[] cards)
    {
        var str = "";
        if (cards == null) return str;
        foreach (Card card in cards)
        {
            if (card == null) continue;
            str += card.ToString() + ", ";
        }

        return str;
    }

    string intToString(int[] nums)
    {
        var str = "";
        if (nums == null) return str;
        foreach (int n in nums)
        {
            if (n == null) continue;
            str += n.ToString() + ", ";
        }

        return str;
    }
}
