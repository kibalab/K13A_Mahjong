
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

    public GameObject UICanvas;
    public Button Pon, Chi, Kkan, Rich, Ron, Tsumo, Skip;

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

        foreach(UIButton button in uiButton)
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
        if (uiContext == null) { return; }

        if (uiContext.IsChanged && isMyTable)
        {
            uiContext.IsChanged = false;

            if (uiContext.IsChiable)
            { //Chi �� �̷��� �������� ���� ���ߵǴ°� �ʹ� �ȴ�...
                switch (uiContext.chiableCards.Length) {
                    case 0 :
                        break;
                    case 1 :
                        ActiveButton("Chi");
                        break;
                    case 2 :
                        ActiveButton("Chi");
                        ActiveButton("ChiSelect"); // ���Ƿ� ������Ʈ�̸� �־��
                        setChiSelectButton(2);
                        break;
                    case 3:
                        ActiveButton("Chi");
                        ActiveButton("ChiSelect");
                        setChiSelectButton(3);
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

    public void setChiSelectButton(int size)
    {
        for(var i = 0; i<size; i++)
        {
            GameObject g = UICanvas.transform.Find("ChiSelect").GetChild(size).gameObject;
            for(var j = 0; j<3; j++)
            {
                g.transform.GetChild(j).GetComponent<Image>().sprite = ((Card[])uiContext.chiableCards[i])[j].transform.Find("Display").GetComponent<SpriteRenderer>().sprite;
            }
        }
    }


    public void ActiveButton(string uiName)
    {
        GameObject g = UICanvas.transform.Find(uiName).gameObject;
        UICanvas.SetActive(true);
        g.gameObject.SetActive(true);
    }

    public void _DisableButton()
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

    public void _ClickButton()
    {
        Debug.Log("[UION] ClickEvent PlayerTurn : " + playerTurn + ", UIName : " + UIName);
        if (UIName.Contains("chiSelect_"))
        {
            ((NakiInput)((object)inputEvent)).chiCards = uiContext.chiableCards[int.Parse(UIName.Replace("chiSelect_", ""))];
        }
        inputEvent.Set(SelectedCard, UIName, playerTurn);
        eventQueue.Enqueue(inputEvent);
    }
}
