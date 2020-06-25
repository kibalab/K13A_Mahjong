
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class UIManager : UdonSharpBehaviour
{
    [SerializeField] public int PlayerIndex;
    [SerializeField] public InputEvent InputEvent;
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public UIContext UIContext;
    [SerializeField] public GameObject UICanvas;
    [SerializeField] public CardSprites CardSprites;

    // 플레이어가 [참여] 버튼을 누를 때 local에만 할당된다.
    // 일단은 테스트를 위해서 true로 둠
    private bool isMyTable = true;

    void Start()
    {
        DisableButtonAll();
        UIContext.Clear();
    }

    void Update()
    {
        if (UIContext == null) { return; }

        if (isMyTable)
        {
            if (UIContext.IsChiable)
            {
                ActiveButton("Chi");
                OpenChiSelect();
            }
            if (UIContext.IsPonable) ActiveButton("Pon");
            if (UIContext.IsKkanable) ActiveButton("Kkan");
            if (UIContext.IsRiichable) ActiveButton("Rich");
            if (UIContext.IsTsumoable) ActiveButton("Tsumo");
            if (UIContext.IsRonable) ActiveButton("Ron");
            if (UIContext.IsAnythingActived()) ActiveButton("Skip");
            else { DisableButtonAll(); }
        }
    }

    void OpenChiSelect()
    {
        var count = UIContext.ChiableCount;
        if (count != 0) SetChiSelectButton(count);
    }

    void SetChiSelectButton(int size)
    {
        for (var i = 0; i < size; i++)
        {
            var tr = UICanvas.transform.Find("ChiSelect").GetChild(i);
            var spriteNames = UIContext.GetCardSpriteNames(i);

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

    public void OnClick(string clickedUIName)
    {
        if (Networking.LocalPlayer == null)
        {
            ClickButtonForUnityTests(clickedUIName);
        }
        else
        {
            if (clickedUIName == "Chi" && UIContext.ChiableCount > 1)
            {
                ActiveButton("ChiSelect");
                ActiveButton("Skip");
            }
            else if (clickedUIName == "Chi" || clickedUIName.StartsWith("chiSelect"))
            {
                ClickChiSelect(clickedUIName);
            }
            else
            {
                ClickOthers(clickedUIName);
            }
        }
    }

    void ClickChiSelect(string uiName)
    {
        var funcName = GetChiFuncByUIName(uiName);
        SendCustomNetworkEvent(NetworkEventTarget.Owner, funcName);
    }

    string GetChiFuncByUIName(string uiName)
    {
        switch (uiName)
        {
            case "Chi": return nameof(_ClickChiSelect1);
            case "chiSelect_1": return nameof(_ClickChiSelect1);
            case "chiSelect_2": return nameof(_ClickChiSelect2);
            case "chiSelect_3": return nameof(_ClickChiSelect3);
            default: return null;
        }
    }

    public void _ClickChiSelect1() { SetChiEvent(UIContext.ChiableIndex1); }
    public void _ClickChiSelect2() { SetChiEvent(UIContext.ChiableIndex2); }
    public void _ClickChiSelect3() { SetChiEvent(UIContext.ChiableIndex3); }

    void SetChiEvent(Vector2 chiIndex)
    {
        InputEvent.SetChiEvent(chiIndex, "Chi", PlayerIndex);
        EventQueue.Enqueue(InputEvent);
    }

    public void ClickOthers(string uiName)
    {
        var funcName = $"_Click{uiName}";
        SendCustomNetworkEvent(NetworkEventTarget.Owner, funcName);
    }

    public void _ClickPon() { SetUIEvent("Pon"); }
    public void _ClickKkan() { SetUIEvent("Kkan"); }
    public void _ClickRon() { SetUIEvent("Ron"); }
    public void _ClickTsumo() { SetUIEvent("Tsumo"); }
    public void _ClickSkip() { SetUIEvent("Skip"); }

    void SetUIEvent(string eventName)
    {
        InputEvent.SetUIEvent(eventName, PlayerIndex);
        EventQueue.Enqueue(InputEvent);
    }

    public void ClickButtonForUnityTests(string uiName)
    {
        if (uiName == "Chi" && UIContext.ChiableCount > 1)
        {
            ActiveButton("ChiSelect");
            ActiveButton("Skip");
        }
        else if (uiName == "Chi" && UIContext.ChiableCount == 1)
        {
            InputEvent.SetChiEvent(UIContext.ChiableIndex1, uiName, PlayerIndex);
            EventQueue.Enqueue(InputEvent);
        }
        else if (uiName.StartsWith("chiSelect"))
        {
            var chiYamaIndexes = GetChiIndexByUIName(uiName);
            InputEvent.SetChiEvent(chiYamaIndexes, "Chi", PlayerIndex);
            EventQueue.Enqueue(InputEvent);
        }
        else
        {
            InputEvent.SetUIEvent(uiName, PlayerIndex);
            EventQueue.Enqueue(InputEvent);
        }
    }

    Vector2 GetChiIndexByUIName(string uiName)
    {
        switch (uiName)
        {
            case "chiSelect_1": return UIContext.ChiableIndex1;
            case "chiSelect_2": return UIContext.ChiableIndex2;
            case "chiSelect_3": return UIContext.ChiableIndex3;
        }

        return Vector2.zero;
    }
}
