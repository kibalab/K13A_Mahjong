
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class UIManager : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string UIName;
    [UdonSynced(UdonSyncMode.None)] public int SelectedCard;
    [UdonSynced(UdonSyncMode.None)] public int playerTurn;

    /*LinkedInInspector*/ public GameObject UICanvas;
    /*LinkedInInspector*/ public CardSprites CardSprites;

    private UIContext uiContext;
    private EventQueue eventQueue;
    private InputEvent inputEvent;

    // �÷��̾ [����] ��ư�� ���� �� local���� �Ҵ�ȴ�.
    // �ϴ��� �׽�Ʈ�� ���ؼ� true�� ��
    private bool isMyTable = true;

    public void Initialize(InputEvent inputEvent, EventQueue eventQueue, UIContext uiContext)
    {
        this.uiContext = uiContext;
        this.eventQueue = eventQueue;
        this.inputEvent = inputEvent;

        UIButton[] uiButton = UICanvas.GetComponentsInChildren<UIButton>();

        foreach (UIButton button in uiButton)
        {
            button.Initialize(this);
        }

        if (Networking.LocalPlayer != null)
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "_DisableButton");
        }
        else
        {
            _DisableButton();
        }
    }

    void Update()
    {
        if (uiContext.IsChanged && isMyTable)
        {
            uiContext.IsChanged = false;

            if (uiContext.IsChiable)
            { //Chi �� �̷��� �������� ���� ���ߵǴ°� �ʹ� �ȴ�...
                switch (uiContext.ChiableCount)
                {
                    case 0:
                        ActiveButton("Chi");
                        ActiveButton("ChiSelect"); // ���Ƿ� ������Ʈ�̸� �־��
                        SetChiSelectButton(1);
                        break;
                    case 1:
                        ActiveButton("Chi");
                        ActiveButton("ChiSelect");
                        SetChiSelectButton(2);
                        break;
                    case 2:
                        ActiveButton("Chi");
                        ActiveButton("ChiSelect");
                        SetChiSelectButton(3);
                        break;
                }
            }
            if (uiContext.IsPonable) ActiveButton("Pon");
            if (uiContext.IsKkanable) ActiveButton("Kkan");
            if (uiContext.IsRiichable) ActiveButton("Rich");
            if (uiContext.IsTsumoable) ActiveButton("Tsumo");
            if (uiContext.IsRonable) ActiveButton("Ron");
            if (uiContext.IsAnythingActived()) ActiveButton("Skip");
        }
    }

    void SetChiSelectButton(int size)
    {
        for (var i = 0; i < size; i++)
        {
            var tr = UICanvas.transform.Find("ChiSelect").GetChild(i);
            var spriteNames = uiContext.GetCardSpriteNames(i);
            for (var j = 0; j < 2; j++)
            {
                var spriteName = spriteNames[j];
                var sprite = CardSprites.FindSprite(spriteName);

                var image = tr.GetChild(j).GetComponent<Image>();
                image.sprite = sprite;
            }
        }
    }

    void ActiveButton(string uiName)
    {
        GameObject g = UICanvas.transform.Find(uiName).gameObject;
        UICanvas.SetActive(true);
        g.gameObject.SetActive(true);
    }

    void _DisableButton()
    {
        for (var i = 0; i < UICanvas.transform.childCount; i++)
        {
            UICanvas.transform.GetChild(i).gameObject.SetActive(false);
        }
        UICanvas.SetActive(false);
    }

    public void OnClick(string uiName)
    {
        UIName = uiName;
        SelectedCard = -1; // TODO ���⼭ �׼ǿ� ������ ī�带 �־��־�� �Ѵ�.

        if (Networking.LocalPlayer == null)
        {
            _ClickButton();
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, "_ClickButton");
        }

        _DisableButton();
    }

    void _ClickButton()
    {
        Debug.Log("[UION] ClickEvent PlayerTurn : " + playerTurn + ", UIName : " + UIName);

        switch (UIName)
        {
            case "chiSelect_1":
                inputEvent.ChiIndex = new Vector2(uiContext.ChiableIndex1.x, uiContext.ChiableIndex1.y);
                break;
            case "chiSelect_2":
                inputEvent.ChiIndex = new Vector2(uiContext.ChiableIndex2.x, uiContext.ChiableIndex2.y);
                break;
            case "chiSelect_3":
                inputEvent.ChiIndex = new Vector2(uiContext.ChiableIndex3.x, uiContext.ChiableIndex3.y);
                break;
        }

        inputEvent.Set(SelectedCard, UIName, playerTurn);
        eventQueue.Enqueue(inputEvent);
    }
}
