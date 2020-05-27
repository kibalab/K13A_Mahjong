using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class CardManager : UdonSharpBehaviour
{
    private const int FULL_CARD_COUNT = 14;

    public string positionName;
    public UIManager UiManager;
    public GameObject CardPositions;    
    public KList Cards;
    public KList OpenendCards;

    private InputActionEvent inputEvent;
    private GameObject[] cardPoints;

    Transform plusCardPosition;
    HandCalculator handCalculator;
    int myTableNumber;

    public void Initialize(HandCalculator handCalculator, int myTableNumber, EventQueue eq)
    {
        cardPoints = FindPoints();

        this.handCalculator = handCalculator;
        this.myTableNumber = myTableNumber;

        inputEvent = this.gameObject.GetComponentInChildren<InputActionEvent>();
        UiManager = this.gameObject.GetComponentInChildren<UIManager>();

        UiManager.Initialize(eq, inputEvent);
    }

    GameObject[] FindPoints()
    {
        //�迭�� 0~13 �� ����ī�� 14�� �߰�ī��
        var cardPoints = new GameObject[FULL_CARD_COUNT];
        for (int i = 0; i < 14; i++)
        {
            cardPoints[i] = CardPositions.transform.GetChild(i).gameObject;
        }
        plusCardPosition = cardPoints[13].transform;
        return cardPoints;
    }

    public void AddCard(CardComponent newPlusCard) 
    {
        newPlusCard.InputEvent = inputEvent;

        Cards.Add(newPlusCard);
        newPlusCard.SetPosition(plusCardPosition.position, plusCardPosition.rotation);
    }

    public void Discard(CardComponent stashCard)
    {
        var index = Cards.IndexOf(stashCard);
        var removed = (CardComponent)Cards.RemoveAt(index);
        removed.InputEvent.deleteData();

        Cards.Sort();
        SortPosition();
    }

    public bool Contains(CardComponent card)
    {
        return Cards.Contains(card);
    }

    public void SetCards(CardComponent[] pickedCards)
    {
        for (int i = 0; i< pickedCards.Length; ++i)
        {
            var pointTransform = cardPoints[i].transform;
            var pickedCard = pickedCards[i];

            Cards.Add(pickedCards[i]);
            pickedCard.InputEvent = inputEvent;
            pickedCard.SetPosition(pointTransform.position, pointTransform.transform.rotation);
        }

        Cards.Sort();
        SortPosition();
    }

    public void Pickupable(bool b)
    {
        foreach (CardComponent card in Cards.Clone())
        {
            card.gameObject.GetComponent<BoxCollider>().enabled = b;
        }
    }

    public bool CheckChiable(CardComponent discardedCard)
    {
        var chiable = handCalculator.IsChiable((CardComponent[])Cards.Clone(), discardedCard);
        if (chiable)
        {
            // ���� �÷��̾� VRCPlayerApi�� ������ ������ �Լ��� ����
            UiManager.ActiveButton("Chi", null, myTableNumber);
        }
        return chiable;
    }

    public bool CheckPonable(CardComponent discardedCard)
    {
        var ponable = handCalculator.IsPonable((CardComponent[])Cards.Clone(), discardedCard);
        if (ponable)
        {
            // ���� �÷��̾� VRCPlayerApi�� ������ ������ �Լ��� ����
            UiManager.ActiveButton("Pon", null, myTableNumber);
        }
        return ponable;
    }

    public bool CheckKkanable(CardComponent discardedCard)
    {
        var kkanable = handCalculator.IsKkanable((CardComponent[])Cards.Clone(), discardedCard);
        if (kkanable)
        {
            // ���� �÷��̾� VRCPlayerApi�� ������ ������ �Լ��� ����
            UiManager.ActiveButton("Kkan", null, myTableNumber);
        }
        return kkanable;
    }

    void SortPosition()
    {
        for (var k = 0; k < Cards.Count(); k++) // ���α�����
        {
            var card = (CardComponent)Cards.At(k);
            setPointPosition(card, cardPoints[k]);
        }
    }

    public void setPointPosition(CardComponent card, GameObject point)
    {
        card.SetPosition(point.transform.position, point.transform.rotation);
    }
}