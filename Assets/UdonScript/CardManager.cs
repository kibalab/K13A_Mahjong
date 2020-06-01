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

    public void AddCard(Card newPlusCard) 
    {
        newPlusCard.InputEvent = inputEvent;

        Cards.Add(newPlusCard);
        newPlusCard.SetPosition(plusCardPosition.position, plusCardPosition.rotation);
    }

    public void Discard(Card stashCard)
    {
        var index = Cards.IndexOf(stashCard);
        var removed = (Card)Cards.RemoveAt(index);
        removed.InputEvent.deleteData();

        Cards.Sort();
        SortPosition();
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
            pickedCard.InputEvent = inputEvent;
            pickedCard.SetPosition(pointTransform.position, pointTransform.transform.rotation);
        }

        Cards.Sort();
        SortPosition();
    }

    public void Pickupable(bool b)
    {
        foreach (Card card in Cards.Clone())
        {
            card.gameObject.GetComponent<BoxCollider>().enabled = b;
        }
    }

    public bool CheckChiable(Card discardedCard)
    {
        var chiable = handCalculator.IsChiable(GetArray(), discardedCard);
        if (chiable)
        {
            // ���� �÷��̾� VRCPlayerApi�� ������ ������ �Լ��� ����
            UiManager.ActiveButton("Chi", null, myTableNumber);
        }
        return chiable;
    }

    public bool CheckPonable(Card discardedCard)
    {
        var ponable = handCalculator.IsPonable(GetArray(), discardedCard);
        if (ponable)
        {
            // ���� �÷��̾� VRCPlayerApi�� ������ ������ �Լ��� ����
            UiManager.ActiveButton("Pon", null, myTableNumber);
        }
        return ponable;
    }

    public bool CheckKkanable(Card discardedCard)
    {
        var kkanable = handCalculator.IsKkanable(GetArray(), discardedCard);
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
            var card = (Card)Cards.At(k);
            setPointPosition(card, cardPoints[k]);
        }
    }

    public void setPointPosition(Card card, GameObject point)
    {
        card.SetPosition(point.transform.position, point.transform.rotation);
    }

    Card[] GetArray()
    {
        var arr = new Card[Cards.Count()];
        for(var i = 0; i<Cards.Count(); ++i)
        {
            arr[i] = (Card)Cards.At(i);
        }
        return arr;
    }
}