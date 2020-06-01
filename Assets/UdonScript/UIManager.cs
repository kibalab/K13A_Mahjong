
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

    // 플레이어가 [참여] 버튼을 누를 때 local에만 할당된다.
    // 일단은 테스트를 위해서 true로 둠
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

            if (uiContext.IsChiable) ActiveButton("Chi");
            if (uiContext.IsPonable) ActiveButton("Pon");
            if (uiContext.IsKkanable) ActiveButton("Kkan");
            if (uiContext.IsRiichable) ActiveButton("Rich");
            if (uiContext.IsTsumoable) ActiveButton("Tsumo");
            if (uiContext.IsRonable) ActiveButton("Ron");
            if (uiContext.IsAnythingActived()) ActiveButton("Skip");
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
        SelectedCard = -1; // TODO 여기서 액션에 선택할 카드를 넣어주어야 한다.

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

        inputEvent.Set(SelectedCard, UIName, playerTurn);
        eventQueue.Enqueue(inputEvent);
    }
}
