
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandCalculator : UdonSharpBehaviour
{
    public CombinationIterator combitnationIterator;

    public void FindValidCombination(CardComponent[] cards)
    {
        var manGroup = GetCardsByType(cards, "��");
        var souGroup = GetCardsByType(cards, "��");
        var pinGroup = GetCardsByType(cards, "��");



    }


    public CardComponent[] GetCardsByType(CardComponent[] allCards, string type)
    {
        // �ش� type�� ī�� ������ŭ ����Ʈ�� �������... �� ����ۿ� ����...
        var typedCardsCount = GetCardTypeCount(allCards, type);
        var typedCards = new CardComponent[typedCardsCount];
        var index = 0;

        foreach (var card in allCards)
        {
            if (type == card.Type)
            {
                typedCards[index++] = card;
            }
        }

        return SortCard(typedCards);
    }

    int GetCardTypeCount(CardComponent[] cards, string type)
    {
        var count = 0;

        foreach (var card in cards)
        {
            if (type == card.Type)
            {
                count++;
            }
        }

        return count;
    }

    CardComponent[] SortCard(CardComponent[] cards)
    {
        CardComponent temp;

        for (var i = cards.Length -1; i >= 0; i--)
        {
            for (var j = 1; j <= i; j++)
            {
                if (cards[j - 1].NormalCardNumber > cards[j].NormalCardNumber)
                {
                    temp = cards[j - 1];
                    cards[j - 1] = cards[j];
                    cards[j] = temp;
                }
            }
        }

        return cards;
    }
}
