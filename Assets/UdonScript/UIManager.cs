
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

            tr.gameObject.SetActive(true);
            for (var j = 0; j < 2; j++)
            {
                var spriteName = spriteNames[j];
                var sprite = CardSprites.FindSprite(spriteName);

                var image = tr.GetChild(j).GetComponent<Image>();
                image.sprite = sprite;
            }
        }
        for (var i = 2; i >= size; i--)
        {
            UICanvas.transform.Find("ChiSelect").GetChild(i).gameObject.SetActive(false);
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
        if (isRunOnMasterScript)
        {
            Debug.Log("[UION] ClickEvent PlayerTurn : " + PlayerIndex + ", UIName : " + UIName);

            DisableButtonAll();

            if (UIName == "Chi" && uiContext.ChiableCount > 1)
            {
                ActiveButton("ChiSelect");
            }
            else
            {
                var chiYamaIndexes = GetChiIndexByUIName();
                inputEvent.SetChiEvent(chiYamaIndexes, UIName, PlayerIndex);
                eventQueue.Enqueue(inputEvent);
            }
        }
    }

    Vector2 GetChiIndexByUIName()
    {
        switch (UIName)
        {
            case "Chi":         return uiContext.ChiableIndex1;
            case "chiSelect_1": return uiContext.ChiableIndex1;
            case "chiSelect_2": return uiContext.ChiableIndex2;
            case "chiSelect_3": return uiContext.ChiableIndex3;
        }

        return Vector2.zero;
    }
}
