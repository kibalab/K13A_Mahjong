
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
    [UdonSynced(UdonSyncMode.None)] public int PlayerIndex;

    /*LinkedInInspector*/ public GameObject UICanvas;
    /*LinkedInInspector*/ public CardSprites CardSprites;

    private UIContext uiContext;
    public  EventQueue eventQueue;
    private InputEvent inputEvent;

    // 플레이어가 [참여] 버튼을 누를 때 local에만 할당된다.
    // 일단은 테스트를 위해서 true로 둠
    private bool isMyTable = true;
    private bool isInitialized = false;

    // 월드 마스터의 local에서만 true인 항목
    private bool isRunOnMasterScript = false;

    public void Initialize_Master(int playerIndex)
    {
        PlayerIndex = playerIndex;
        isRunOnMasterScript = true;
    }

    public void Initialize_All(InputEvent inputEvent, EventQueue eventQueue, UIContext uiContext)
    {
        this.uiContext = uiContext;
        this.eventQueue = eventQueue;
        this.inputEvent = inputEvent;

        var uiButton = UICanvas.GetComponentsInChildren<UIButton>();

        foreach (UIButton button in uiButton)
        {
            button.Initialize(this);
        }

        DisableButtonAll();

        isInitialized = true;
    }

    void Update()
    {
        if (!isInitialized) { return; }

        if (uiContext.IsChanged && isMyTable)
        {
            uiContext.IsChanged = false;

            // 테스트 해야되서 임시로 비활성화함
            if (uiContext.IsChiable) { OpenChiSelect(); }
            if (uiContext.IsPonable) ActiveButton("Pon");
            if (uiContext.IsKkanable) ActiveButton("Kkan");
            if (uiContext.IsRiichable) ActiveButton("Rich");
            if (uiContext.IsTsumoable) ActiveButton("Tsumo");
            if (uiContext.IsRonable) ActiveButton("Ron");
            if (uiContext.IsAnythingActived()) ActiveButton("Skip");
        }
    }

    void OpenChiSelect()
    {
        switch (uiContext.ChiableCount)
        {
            case 0:
                ActiveButton("Chi");
                //ActiveButton("ChiSelect"); // 임의로 오브젝트이름 넣어둠
                //SetChiSelectButton(1);
                break;
            case 1:
                ActiveButton("Chi");
                //ActiveButton("ChiSelect");
                SetChiSelectButton(1);
                break;
            case 2:
                ActiveButton("Chi");
                //ActiveButton("ChiSelect");
                SetChiSelectButton(2);
                break;
            case 3:
                ActiveButton("Chi");
                //ActiveButton("ChiSelect");
                SetChiSelectButton(3);
                break;
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
        var tr = UICanvas.transform.Find(uiName);
        if (tr == null) { Debug.Log($"{uiName} not exists."); }
        UICanvas.SetActive(true);
        tr.gameObject.SetActive(true);
    }

    public void DisableButtonAll()
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

        if (Networking.LocalPlayer == null)
        {
            _ClickButton();
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.All, "_ClickButton");
        }
    }

    public void _ClickButton()
    {
        if (!isRunOnMasterScript)
        {
            return;
        }

        Debug.Log("[UION] ClickEvent PlayerTurn : " + PlayerIndex + ", UIName : " + UIName);

        switch (UIName)
        {
            case "chiSelect_1":
                UIName = "Chi";
                DisableButtonAll();
                inputEvent.SetChiEvent(uiContext.ChiableIndex1, UIName, PlayerIndex);
                eventQueue.Enqueue(inputEvent);
                break;

            case "chiSelect_2":
                UIName = "Chi";
                DisableButtonAll();
                inputEvent.SetChiEvent(uiContext.ChiableIndex2, UIName, PlayerIndex);
                eventQueue.Enqueue(inputEvent);
                break;

            case "chiSelect_3":
                UIName = "Chi";
                DisableButtonAll();
                inputEvent.SetChiEvent(uiContext.ChiableIndex3, UIName, PlayerIndex);
                eventQueue.Enqueue(inputEvent);
                break;

            case "Chi":
                if (uiContext.ChiableCount > 1)
                {
                    ActiveButton("ChiSelect");
                }
                else
                {
                    inputEvent.SetChiEvent(uiContext.ChiableIndex3, UIName, PlayerIndex);
                    eventQueue.Enqueue(inputEvent);
                }
                break;
        }
    }
}
