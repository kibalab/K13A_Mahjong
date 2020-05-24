
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandCalculator : UdonSharpBehaviour
{
    public CombinationIterator combinationInterator;
    public KList ManGroup;
    public KList SouGroup;
    public KList PinGroup;

    public void FindValidCombination(CardComponent[] cards)
    {
        ManGroup.Clear();
        SouGroup.Clear();
        PinGroup.Clear();

        var copiedCards = SortCardsWithHardCopy(cards);


        var manGroupIndex = GetCardsIndexByType(copiedCards, "만");
        var souGroupIndex = GetCardsIndexByType(copiedCards, "삭");
        var pinGroupIndex = GetCardsIndexByType(copiedCards, "통");
        //GetCardsIndexByType(ManGroup, copiedCards, "만");
        //GetCardsIndexByType(SouGroup, copiedCards, "삭");
        //GetCardsIndexByType(PinGroup, copiedCards, "통");

        PrintGroupedCards(copiedCards, souGroupIndex);
        PrintGroupedCards(copiedCards, manGroupIndex);
        PrintGroupedCards(copiedCards, pinGroupIndex);
        //PrintGroupedCards(ManGroup);
        //PrintGroupedCards(SouGroup);
        //PrintGroupedCards(PinGroup);

        Test(copiedCards, ManGroup);
        Test(copiedCards, SouGroup);
        Test(copiedCards, PinGroup);
    }

    void Test(CardComponent[] cards, KList group)
    {
        var k = 3;
        combinationInterator.Initialize(group.Count(), k);
        while (combinationInterator.GetCombination() != null)
        {
            var combination = combinationInterator.GetCombination();
            var pickedCards = new CardComponent[k];
            for (var i = 0; i < k; ++i)
            {
                var gg = (int)group.At(combination[i]);
                pickedCards[i] = cards[gg];
            }

            if (IsValidMelds(pickedCards))
            {

            }

            if (IsChi(pickedCards))
            {
                Debug.Log("IsChi " + CompToString(pickedCards[0]) + CompToString(pickedCards[1]) + CompToString(pickedCards[2]));
            }

            if (IsPon(pickedCards))
            {
                Debug.Log("IsPon " + CompToString(pickedCards[0]) + CompToString(pickedCards[1]) + CompToString(pickedCards[2]));
            }

            combinationInterator.MoveNext();
        }
    }

    //--------------------------임시

    public int[] GetCardsIndexByType(CardComponent[] allCards, string type)
    {
        var typedCardsCount = GetCardTypeCount(allCards, type);
        var typedCardsIndex = new int[typedCardsCount];
        var index = 0;

        for (var i = 0; i < allCards.Length; ++i)
        {
            if (type == allCards[i].Type)
            {
                typedCardsIndex[index++] = i;
            }
        }
        return typedCardsIndex;
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

    void PrintGroupedCards(CardComponent[] cards, int[] group)
    {
        var str = "";
        foreach (var i in group)
        {
            str += CompToString(cards[i]) + " ";
        }
        Debug.Log(str);
    }
    //-----------------------------

    bool IsValidMelds(CardComponent[] pickedCards)
    {
        return IsChi(pickedCards) || IsPon(pickedCards);
    }

    bool IsChi(CardComponent[] pickedCards)
    {
        if (pickedCards.Length != 3) { return false; }
        return pickedCards[0].CardNumber == pickedCards[1].CardNumber - 1
            && pickedCards[0].CardNumber == pickedCards[2].CardNumber - 2;
    }

    bool IsPon(CardComponent[] pickedCards)
    {
        if (pickedCards.Length != 3) { return false; }
        return pickedCards[0].CardNumber == pickedCards[1].CardNumber
            && pickedCards[0].CardNumber == pickedCards[2].CardNumber;
    }

    string CompToString(CardComponent comp)
    {
        return "(" + comp.Type + ", " + comp.CardNumber + ")";
    }

    public void GetCardsIndexByType2(KList groupList, CardComponent[] allCards, string type)
    {
        foreach (var card in allCards)
        {
            if (type == card.Type)
            {
                groupList.Add(card);
            }
        }
    }

    CardComponent[] SortCardsWithHardCopy(CardComponent[] cards)
    {
        var copies = new CardComponent[cards.Length];
        for(var i=0; i < cards.Length; ++i)
        {
            copies[i] = cards[i];
        }

        CardComponent temp;

        for (var i = cards.Length - 1; i >= 0; i--)
        {
            for (var j = 1; j <= i; j++)
            {
                if (copies[j - 1].NormalCardNumber > copies[j].NormalCardNumber)
                {
                    temp = cards[j - 1];
                    copies[j - 1] = cards[j];
                    copies[j] = temp;
                }
            }
        }
        
        return copies;
    }

    void PrintGroupedCards2(KList groupedCards)
    {
        var str = "";
        for (var i = 0; i < groupedCards.Count(); ++i)
        {
            var card = (CardComponent)groupedCards.At(i);
            str += CompToString(card) + " ";
        }
        Debug.Log(str);
    }
}

