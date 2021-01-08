
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class UIManager : UdonSharpBehaviour
{
    [SerializeField] public int PlayerIndex;
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public UIContext UIContext;
    [SerializeField] public GameObject UICanvas;
    [SerializeField] public GameObject UIButtons;
    [SerializeField] public GameObject ChiSelect;
    [SerializeField] public GameObject StatsUI;
    [SerializeField] public CardSprites CardSprites;
    [SerializeField] public AudioQueue AudioQueue;

    // 플레이어가 [참여] 버튼을 누를 때 local에만 할당된다.
    // 일단은 테스트를 위해서 true로 둠
    private bool isMyTable = true;

    private int AgariableMessageIndex = -1;

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
            if(UIContext.AgariableMessageIndex != AgariableMessageIndex)
            {

                var cards = UIContext.AgariableCards.Split(',');
                var i = 0;
                foreach(var card in cards)
                {
                    var image = StatsUI.transform.GetChild(i++).GetChild(0).GetChild(0).GetComponent<Image>();
                    //image.sprite = CardSprites.FindSprite(card);
                }
                setShantenStats(i);
                AgariableMessageIndex = UIContext.AgariableMessageIndex;
            }

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
            if (UIContext.IsAnythingActived()) { ActiveButton("Skip"); ActiveButton("UIOpenSound"); }
            else { DisableButtonAll(); }
        }
    }

    public void setAgariableViewer(string[] cardNames, string[] cardStats)
    {
        if(Networking.LocalPlayer != null)
        {
            if (Networking.IsMaster)
            {
                //동기화를 위해 만들어둔 공간
            }
        }

        for(var i = 0; i < cardNames.Length; i++)
        {
            var plate = StatsUI.transform.Find(i.ToString());
            var Img = plate.GetChild(0).GetChild(0).GetComponent<Image>();
            var txt = plate.GetChild(1).GetComponent<Text>();
            
            Img.sprite = CardSprites.FindSprite(cardNames[i]);
            txt.text = cardStats[i];
        }
        
    }

    public void setShantenStats(int needCardCount)
    {
        var txt = StatsUI.transform.Find("Shanten").GetComponent<Text>();

        var str = "";
        switch (needCardCount)
        {
            case 0:
                str = "<color=red>텐파이</color>";
                break;
            case 1:
                str = "<color=red>이샹텐</color>";
                break;
            case 2:
                str = "<color=orange>량샹텐</color>";
                break;
            case 3:
                str = "<color=yellow>산샹텐</color>";
                break;
            case 4:
                str = "<color=blue>스샹텐</color>";
                break;
            case 5:
                str = "<color=green>우샹텐</color>";
                break;
        }

        txt.text = str;
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
            var tr = ChiSelect.transform.GetChild(i);
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
            ChiSelect.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    void ActiveButton(string uiName)
    {
        var tr = UIButtons.transform.Find(uiName);
        if (tr == null) { Debug.Log($"{uiName} not exists."); }
        //AudioQueue.AddQueue("UIOpenSound");
        //UICanvas.SetActive(true);
        tr.gameObject.SetActive(true);
    }

    void ActiveChiSelect()
    {
        ChiSelect.SetActive(true);
    }

    public void DisableButtonAll()
    {
        for (var i = 0; i < UIButtons.transform.childCount; i++)
        {
            UIButtons.transform.GetChild(i).gameObject.SetActive(false);
            ChiSelect.SetActive(false);
        }
        //UICanvas.SetActive(false);
    }

    public void OnClick(string clickedUIName)
    {
        if (clickedUIName == "Chi" && UIContext.ChiableCount > 1)
        {
            ActiveChiSelect();
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

    void ClickChiSelect(string uiName)
    {
        var funcName = GetChiFuncByUIName(uiName);
        RequestCallFunctionToOwner(funcName);
    }

    string GetChiFuncByUIName(string uiName)
    {
        switch (uiName)
        {
            case "Chi": return nameof(l_ClickChiSelect1);
            case "chiSelect_1": return nameof(l_ClickChiSelect1);
            case "chiSelect_2": return nameof(l_ClickChiSelect2);
            case "chiSelect_3": return nameof(l_ClickChiSelect3);
            default: return null;
        }
    }

    public void l_ClickChiSelect1() { SetChiEvent(UIContext.ChiableIndex1); }
    public void l_ClickChiSelect2() { SetChiEvent(UIContext.ChiableIndex2); }
    public void l_ClickChiSelect3() { SetChiEvent(UIContext.ChiableIndex3); }

    void SetChiEvent(Vector2 chiIndex)
    {
        EventQueue.SetChiEvent(chiIndex, "Chi", PlayerIndex);
    }

    public void ClickOthers(string uiName)
    {
        var funcName = $"l_Click{uiName}";
        RequestCallFunctionToOwner(funcName);
    }
    public void l_ClickPon() { SetUIEvent("Pon"); }
    public void l_ClickKkan() { SetUIEvent("Kkan"); }
    public void l_ClickRon() { SetUIEvent("Ron"); }
    public void l_ClickTsumo() { SetUIEvent("Tsumo"); }
    public void l_ClickSkip() { SetUIEvent("Skip"); }
    public void l_ClickRich() { SetUIEvent("Rich"); }

    public void l_ClickTsumoCut() { SetUIEvent("TsumoCut"); }
    public void l_ClickAutoAgari() { SetUIEvent("AutoAgari"); }
    public void l_ClickNoNaki() { SetUIEvent("NoNaki"); }
    public void l_ClickNotSort() { SetUIEvent("NotSort"); }

    void SetUIEvent(string eventName)
    {
        EventQueue.SetUIEvent(eventName, PlayerIndex);
    }

    void RequestCallFunctionToOwner(string funcName)
    {
        if (Networking.LocalPlayer == null)
        {
            SendCustomEvent(funcName);
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, funcName);
        }
    }


}
