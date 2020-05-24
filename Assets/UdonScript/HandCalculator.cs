
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class HandCalculator : UdonSharpBehaviour
{
    //public bool IgnoreTests = false;

    //public CombinationIterator combinationInterator;
    //public KList ManGroup;
    //public KList SouGroup;
    //public KList PinGroup;

    //public KList AvaliableMeldsGroup;
    //public KList Stack;

    //public CardComponent[] TestComponents;

    //public void Start()
    //{
    //    if (IgnoreTests) { return; }

    //    Debug.Log("--- HandCalculator TEST ---");

    //    TestComponents = GetComponentsInChildren<CardComponent>();

    //    ManGroup.Clear();
    //    SouGroup.Clear();
    //    PinGroup.Clear();
    //    var testSet = new CardComponent[]
    //    {
    //            TEST__SetTestData(TestComponents[0], "만", 1, 0),
    //            TEST__SetTestData(TestComponents[1], "만", 2, 1),
    //            TEST__SetTestData(TestComponents[2], "만", 3, 2),
    //            TEST__SetTestData(TestComponents[3], "만", 3, 3),
    //            TEST__SetTestData(TestComponents[4], "삭", 5, 4),
    //            TEST__SetTestData(TestComponents[5], "삭", 5, 5),
    //            TEST__SetTestData(TestComponents[6], "삭", 5, 6),
    //            TEST__SetTestData(TestComponents[7], "통", 7, 7),
    //            TEST__SetTestData(TestComponents[8], "통", 8, 8),
    //            TEST__SetTestData(TestComponents[9], "통", 9, 9)
    //    };

    //    GetCardsIndexByType(ManGroup, testSet, "만");
    //    GetCardsIndexByType(SouGroup, testSet, "삭");
    //    GetCardsIndexByType(PinGroup, testSet, "통");

    //    if (ManGroup.Count() != 4) Debug.Log("man grouping error");
    //    if (SouGroup.Count() != 3) Debug.Log("sou grouping error");
    //    if (PinGroup.Count() != 3) Debug.Log("pin grouping error");

    //    if (!Test__GetMeldsCount(ManGroup, 2, 0)) Debug.Log("Find chi-pon error 1"); ;
    //    if (!Test__GetMeldsCount(SouGroup, 0, 1)) Debug.Log("Find chi-pon error 2"); ;
    //    if (!Test__GetMeldsCount(PinGroup, 1, 0)) Debug.Log("Find chi-pon error 2"); ;

    //    ManGroup.Clear();
    //    SouGroup.Clear();
    //    PinGroup.Clear();

    //    testSet = new CardComponent[]
    //    {
    //            TEST__SetTestData(TestComponents[0], "만", 1, 0),
    //            TEST__SetTestData(TestComponents[1], "만", 1, 1),
    //            TEST__SetTestData(TestComponents[2], "만", 2, 2),
    //            TEST__SetTestData(TestComponents[3], "만", 2, 3),
    //            TEST__SetTestData(TestComponents[4], "만", 3, 4),
    //            TEST__SetTestData(TestComponents[5], "만", 3, 5)
    //    };

    //    GetCardsIndexByType(ManGroup, testSet, "만");

    //    // 전체 집합에서 가능한 슌쯔 커쯔 찾기
    //    var avaliableMeldGroups = FindAll(ManGroup);

    //    // 위의 조합은 슌쯔가 2개 나올 것
    //    if (!TEST__EstimateChiPonCount(avaliableMeldGroups, 2, 0)) Debug.Log("error");

    //    Debug.Log("if nothing appeared above, test success");
    //}

    //bool TEST__EstimateChiPonCount(object[] avaliableMeldGroups, int estimatedChiCount, int estimatedPonCount)
    //{
    //    var maxCount = -1;
    //    var maxIndex = -1;
    //    for (var i = 0; i < avaliableMeldGroups.Length; ++i)
    //    {
    //        var list = ((object[])avaliableMeldGroups[i]);
    //        if (list.Length > maxCount)
    //        {
    //            maxCount = list.Length;
    //            maxIndex = i;
    //        }
    //    }

    //    var chiCount = 0;
    //    var ponCount = 0;

    //    foreach (CardComponent[] subGroup in (object[])avaliableMeldGroups[maxIndex])
    //    {
    //        if (IsChi(subGroup)) { chiCount++; }
    //        if (IsPon(subGroup)) { ponCount++; }
    //    }

    //    return estimatedChiCount == chiCount && estimatedPonCount == ponCount;
    //}

    //CardComponent TEST__SetTestData(CardComponent card, string type, int cardNumber, int normalCardNumber)
    //{
    //    card.Type = type;
    //    card.CardNumber = cardNumber;
    //    card.NormalCardNumber = normalCardNumber;
    //    return card;
    //}

    //bool Test__GetMeldsCount(KList group, int estimatedChiCount, int estimatedPonCount)
    //{
    //    var k = 3;
    //    var chi = 0;
    //    var pon = 0;

    //    var combinations = combinationInterator.GetCombinationAll(group.Count(), k);
    //    foreach (var obj in combinations)
    //    {
    //        var combination = (int[])obj;
    //        var pickedCards = new CardComponent[k];
    //        for (var i = 0; i < k; ++i)
    //        {
    //            var comp = group.At(combination[i]);
    //            pickedCards[i] = (CardComponent)comp;
    //        }

    //        if (IsChi(pickedCards)) { chi++; }
    //        if (IsPon(pickedCards)) { pon++; }
    //    }

    //    return estimatedChiCount == chi && estimatedPonCount == pon;
    //}

    //public void FindValidCombination(CardComponent[] cards)
    //{
    //    ManGroup.Clear();
    //    SouGroup.Clear();
    //    PinGroup.Clear();

    //    var copiedCards = SortCardsWithHardCopy(cards);

    //    GetCardsIndexByType(ManGroup, copiedCards, "만");
    //    GetCardsIndexByType(SouGroup, copiedCards, "삭");
    //    GetCardsIndexByType(PinGroup, copiedCards, "통");

    //    PrintMeldGroups(FindAll(ManGroup));
    //    PrintMeldGroups(FindAll(SouGroup));
    //    PrintMeldGroups(FindAll(PinGroup));
    //}

    //void PrintMeldGroups(object[] meldGroups)
    //{
    //    if (meldGroups.Length == 0) return;
    //    Debug.Log("--- 가능한 슌쯔/커쯔 ---");
    //    foreach (object[] meldGroup in meldGroups)
    //    {
    //        var str = "";
    //        foreach (CardComponent[] meld in meldGroup)
    //        {
    //            str += CompToStr(meld[0]) + CompToStr(meld[2]) + CompToStr(meld[1]);
    //        }
    //        Debug.Log(str);
    //    }
    //}

    //string CompToStr(CardComponent comp)
    //{
    //    return $"({comp.Type}, {comp.CardNumber})";
    //}

    //object[] FindAll(KList group)
    //{
    //    AvaliableMeldsGroup.Clear();

    //    var k = 3;
    //    var originGroup = ObjectsToCardComponents(group.Clone());
    //    var maxAvliableMeldsGroupCount = originGroup.Length / 3;

    //    Stack.Clear();
    //    Stack.Add(originGroup);

    //    while (Stack.Count() != 0)
    //    {
    //        var top = (CardComponent[])Stack.RemoveLast();
    //        if (top.Length > k)
    //        {
    //            var combinations = combinationInterator.GetCombinationAll(top.Length, k);
    //            foreach (int[] combination in combinations)
    //            {
    //                var pickedCards = new CardComponent[k];
    //                for (var i = 0; i < k; ++i)
    //                {
    //                    var comp = group.At(combination[i]);
    //                    pickedCards[i] = (CardComponent)comp;
    //                }

    //                Stack.Add(pickedCards);

    //                var exception = Except(top, pickedCards);
    //                if (exception.Length >= 3)
    //                {
    //                    Stack.Add(exception);
    //                }
    //            }
    //        }
    //        else if (IsValidMelds(top))
    //        {
    //            var avaliableMelds = new object[maxAvliableMeldsGroupCount];
    //            avaliableMelds[0] = top;
    //            var count = 1;
    //            while ((count + 1) * k <= originGroup.Length)
    //            {
    //                var next = (CardComponent[])Stack.RemoveLast();
    //                if (IsValidMelds(next))
    //                {
    //                    avaliableMelds[count++] = next;
    //                }
    //            }

    //            AvaliableMeldsGroup.Add(FitArray(avaliableMelds, count));
    //        }
    //    }

    //    // return object[object[CardComponent[]]
    //    return AvaliableMeldsGroup.Clone();
    //}

    //bool IsValidMelds(CardComponent[] pickedCards)
    //{
    //    return IsChi(pickedCards) || IsPon(pickedCards);
    //}

    //bool IsChi(CardComponent[] pickedCards)
    //{
    //    if (pickedCards.Length != 3) { return false; }
    //    return pickedCards[0].CardNumber == pickedCards[1].CardNumber - 1
    //        && pickedCards[0].CardNumber == pickedCards[2].CardNumber - 2;
    //}

    //bool IsPon(CardComponent[] pickedCards)
    //{
    //    if (pickedCards.Length != 3) { return false; }
    //    return pickedCards[0].CardNumber == pickedCards[1].CardNumber
    //        && pickedCards[0].CardNumber == pickedCards[2].CardNumber;
    //}

    //public void GetCardsIndexByType(KList groupList, CardComponent[] allCards, string type)
    //{
    //    foreach (var card in allCards)
    //    {
    //        if (type == card.Type)
    //        {
    //            groupList.Add(card);
    //        }
    //    }
    //}

    //CardComponent[] GetHardCopy(CardComponent[] cards)
    //{
    //    var copies = new CardComponent[cards.Length];
    //    for (var i = 0; i < cards.Length; ++i)
    //    {
    //        copies[i] = cards[i];
    //    }
    //    return copies;
    //}

    //CardComponent[] SortCardsWithHardCopy(CardComponent[] cards)
    //{
    //    var copies = GetHardCopy(cards);

    //    CardComponent temp;

    //    for (var i = cards.Length - 1; i >= 0; i--)
    //    {
    //        for (var j = 1; j <= i; j++)
    //        {
    //            if (copies[j - 1].NormalCardNumber > copies[j].NormalCardNumber)
    //            {
    //                temp = cards[j - 1];
    //                copies[j - 1] = cards[j];
    //                copies[j] = temp;
    //            }
    //        }
    //    }

    //    return copies;
    //}
    //CardComponent[] Except(CardComponent[] targets, CardComponent[] comps)
    //{
    //    var newObjs = new CardComponent[targets.Length];
    //    var index = 0;
    //    foreach (var target in targets)
    //    {
    //        var isContains = false;
    //        foreach (var comp in comps)
    //        {
    //            if (comp == target)
    //            {
    //                isContains = true;
    //                break;
    //            }
    //        }

    //        if (!isContains)
    //        {
    //            newObjs[index] = target;
    //            ++index;
    //        }
    //    }

    //    return FitArray_CardComponent(newObjs, index);
    //}

    //object[] FitArray(object[] raw, int count)
    //{
    //    var fitObjs = new object[count];
    //    for (var i = 0; i < count; ++i)
    //    {
    //        fitObjs[i] = raw[i];
    //    }
    //    return fitObjs;
    //}


    //CardComponent[] FitArray_CardComponent(CardComponent[] raw, int count)
    //{
    //    var fitObjs = new CardComponent[count];
    //    for (var i = 0; i < count; ++i)
    //    {
    //        fitObjs[i] = raw[i];
    //    }
    //    return fitObjs;
    //}

    //CardComponent[] ObjectsToCardComponents(object[] objects)
    //{
    //    var cardComponents = new CardComponent[objects.Length];
    //    for (var i = 0; i < objects.Length; ++i)
    //    {
    //        cardComponents[i] = (CardComponent)objects[i];
    //    }
    //    return cardComponents;
    //}
}