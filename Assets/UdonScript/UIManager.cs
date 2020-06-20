
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

public class UIManager : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string UIName;

    [SerializeField] public int PlayerIndex;
    [SerializeField] public InputEvent InputEvent;
    [SerializeField] public EventQueue EventQueue;
    [SerializeField] public UIContext UIContext;
    [SerializeField] public GameObject UICanvas;
    [SerializeField] public CardSprites CardSprites;

    // �÷��̾ [����] ��ư�� ���� �� local���� �Ҵ�ȴ�.
    // �ϴ��� �׽�Ʈ�� ���ؼ� true�� ��
    private bool isMyTable = true;

    // ���� �������� local������ true�� �׸�
    private bool isRunOnMasterScript = false;

    private bool isPrevFrameUISynced;

    public void Initialize()
    {
        isRunOnMasterScript = true;
    }

    void Start()
    {
        DisableButtonAll();
        UIContext.Clear();
    }

    void Update()
    {
        var now = Time.time;

        var isSyncTime = now < UIContext.SyncEndTime;

        if (isSyncTime && isMyTable)
        {
            if (UIContext.IsChiable) { OpenChiSelect(); }
            if (UIContext.IsPonable) ActiveButton("Pon");
            if (UIContext.IsKkanable) ActiveButton("Kkan");
            if (UIContext.IsRiichable) ActiveButton("Rich");
            if (UIContext.IsTsumoable) ActiveButton("Tsumo");
            if (UIContext.IsRonable) ActiveButton("Ron");
            if (UIContext.IsAnythingActived()) ActiveButton("Skip");
        }

        isPrevFrameUISynced = isSyncTime;
    }

    void OpenChiSelect()
    {
        switch (UIContext.ChiableCount)
        {
            case 0:
                ActiveButton("Chi");
                //ActiveButton("ChiSelect"); // ���Ƿ� ������Ʈ�̸� �־��
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

            if (UIName == "Chi" && UIContext.ChiableCount > 1)
            {
                ActiveButton("ChiSelect");
                ActiveButton("Skip");
            }
            else if (UIName == "Chi" && UIContext.ChiableCount == 1)
            {
                InputEvent.SetChiEvent(UIContext.ChiableIndex1, UIName, PlayerIndex);
                EventQueue.Enqueue(InputEvent);
            }
            else if (UIName.StartsWith("chiSelect"))
            {
                var chiYamaIndexes = GetChiIndexByUIName();
                InputEvent.SetChiEvent(chiYamaIndexes, "Chi", PlayerIndex);
                EventQueue.Enqueue(InputEvent);
            }
            else
            {
                InputEvent.SetUIEvent(UIName, PlayerIndex);
                EventQueue.Enqueue(InputEvent);
            }
        }
    }

    Vector2 GetChiIndexByUIName()
    {
        switch (UIName)
        {
            case "chiSelect_1": return UIContext.ChiableIndex1;
            case "chiSelect_2": return UIContext.ChiableIndex2;
            case "chiSelect_3": return UIContext.ChiableIndex3;
        }

        return Vector2.zero;
    }
}
