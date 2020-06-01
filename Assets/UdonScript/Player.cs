using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Player : UdonSharpBehaviour
{
    private const int FULL_CARD_COUNT = 14;

    /*LinkedInInspector*/ public UIManager UiManager;
    /*LinkedInInspector*/ public GameObject CardPositions;
    /*LinkedInInspector*/ public KList Cards;
    /*LinkedInInspector*/ public KList OpenendCards;
    /*LinkedInInspector*/ public InputEvent InputEvent;

    private GameObject[] cardPoints;
    private Transform stashPositions;
    private Transform plusCardPosition;
    private HandCalculator HandCalculator;

    int myTableNumber;

    int[] stashedCards;
    int stashedCardIndex;

    public void Initialize(int myTableNumber, EventQueue eq, Transform stashPositions)
    {
        cardPoints = FindPoints();

        this.myTableNumber = myTableNumber;
        this.stashPositions = stashPositions;

        UiManager = this.gameObject.GetComponentInChildren<UIManager>();

        UiManager.Initialize(eq);

        stashedCards = new int[34];
        stashedCardIndex = 0;
    }

    GameObject[] FindPoints()
    {
        //배열의 0~13 은 소유카드 14는 추가카드
        var cardPoints = new GameObject[FULL_CARD_COUNT];
        for (int i = 0; i < 14; i++)
        {
            cardPoints[i] = CardPositions.transform.GetChild(i).gameObject;
        }
        plusCardPosition = cardPoints[13].transform;
        return cardPoints;
    }

    public void AddCard(Card newPlusCard) 
    {
        newPlusCard.InputEvent = InputEvent;

        Cards.Add(newPlusCard);
        newPlusCard.SetPosition(plusCardPosition.position, plusCardPosition.rotation);
    }

    public void Discard(Card stashCard)
    {
        var index = Cards.IndexOf(stashCard);
        Cards.RemoveAt(index);

        stashedCards[stashCard.GlobalIndex]++;
        stashedCardIndex++;

        var point = stashPositions.GetChild(stashedCardIndex);
        stashCard.SetPosition(point.position, point.rotation);
        stashCard.SetColliderActivate(false);

        Cards.Sort();
        SortPosition();
    }

    public void CheckNakiable(Card card)
    {
        // TODO
    }

    public bool IsUIActived()
    {
        return true; // TODO
    }

    public bool Contains(Card card)
    {
        return Cards.Contains(card);
    }

    public void SetCards(Card[] pickedCards)
    {
        for (int i = 0; i< pickedCards.Length; ++i)
        {
            var pointTransform = cardPoints[i].transform;
            var pickedCard = pickedCards[i];

            Cards.Add(pickedCards[i]);
            pickedCard.InputEvent = InputEvent;
            pickedCard.SetPosition(pointTransform.position, pointTransform.transform.rotation);
        }

        Cards.Sort();
        SortPosition();
    }

    public void SetColliderActive(bool active)
    {
        foreach (Card card in Cards.Clone())
        {
            card.gameObject.GetComponent<BoxCollider>().enabled = active;
        }
    }

    void SortPosition()
    {
        for (var k = 0; k < Cards.Count(); k++) // 새로구현함
        {
            var card = (Card)Cards.At(k);
            var cardPoint = cardPoints[k];
            card.SetPosition(cardPoint.transform.position, cardPoint.transform.rotation);
        }
    }
}