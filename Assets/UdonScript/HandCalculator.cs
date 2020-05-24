
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
public class HandCalculator : UdonSharpBehaviour
{
    public bool IgnoreTests = false;

    public CombinationIterator combinationInterator;
    public KList ManGroup;
    public KList SouGroup;
    public KList PinGroup;

    public CardComponent[] TestComponents;

    public void Start()
    {
        if (IgnoreTests) { return; }

        Debug.Log("--- HandCalculator TEST ---");

        TestComponents = GetComponentsInChildren<CardComponent>();
        if (TestComponents.Length != 10) Debug.Log("test data error");

        ManGroup.Clear();
        SouGroup.Clear();
        PinGroup.Clear();
        var testSet = new CardComponent[]
        {
                SetTestData(TestComponents[0], "만", 1),
                SetTestData(TestComponents[1], "만", 2),
                SetTestData(TestComponents[2], "만", 3),
                SetTestData(TestComponents[3], "만", 3),
                SetTestData(TestComponents[4], "삭", 5),
                SetTestData(TestComponents[5], "삭", 5),
                SetTestData(TestComponents[6], "삭", 5),
                SetTestData(TestComponents[7], "통", 7),
                SetTestData(TestComponents[8], "통", 8),
                SetTestData(TestComponents[9], "통", 9)
        };

        GetCardsIndexByType(ManGroup, testSet, "만");
        GetCardsIndexByType(SouGroup, testSet, "삭");
        GetCardsIndexByType(PinGroup, testSet, "통");

        if (ManGroup.Count() != 4) Debug.Log("man grouping error");
        if (SouGroup.Count() != 3) Debug.Log("sou grouping error");
        if (PinGroup.Count() != 3) Debug.Log("pin grouping error");

        if (!TestMeldsCount(ManGroup, 2, 0)) Debug.Log("Find chi-pon error 1"); ;
        if (!TestMeldsCount(SouGroup, 0, 1)) Debug.Log("Find chi-pon error 2"); ;
        if (!TestMeldsCount(PinGroup, 1, 0)) Debug.Log("Find chi-pon error 2"); ;

        Debug.Log("if nothing appeared above, test success");
    }

    CardComponent SetTestData(CardComponent card, string type, int cardNumber)
    {
        card.Type = type;
        card.CardNumber = cardNumber;
        return card;
    }

    bool TestMeldsCount(KList group, int estimatedChiCount, int estimatedPonCount)
    {
        var k = 3;
        var chi = 0;
        var pon = 0;

        combinationInterator.Initialize(group.Count(), k);
        while (combinationInterator.GetCombination() != null)
        {
            var combination = combinationInterator.GetCombination();
            var pickedCards = new CardComponent[k];
            for (var i = 0; i < k; ++i)
            {
                pickedCards[i] = (CardComponent)group.At(combination[i]);
            }

            if (IsChi(pickedCards)) { chi++; }
            if (IsPon(pickedCards)) { pon++; }

            combinationInterator.MoveNext();
        }

        return estimatedChiCount == chi && estimatedPonCount == pon;
    }

    public void FindValidCombination(CardComponent[] cards)
    {
        var copiedCards = SortCardsWithHardCopy(cards);

        GetCardsIndexByType(ManGroup, copiedCards, "만");
        GetCardsIndexByType(SouGroup, copiedCards, "삭");
        GetCardsIndexByType(PinGroup, copiedCards, "통");

        // TODO
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

    public void GetCardsIndexByType(KList groupList, CardComponent[] allCards, string type)
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
        for (var i = 0; i < cards.Length; ++i)
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
}