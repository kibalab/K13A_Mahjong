
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputEvent : UdonSharpBehaviour
{
    public bool isInputBlocked = false;
    public string EventType;
    public int PlayerIndex;
    public int DiscardedCardYamaIndex;
    public Vector2 ChiIndex;

    public void SetUIEvent(string uiName, int playerIndex)
    {
        if (!isInputBlocked)
        {
            EventType = uiName;
            PlayerIndex = playerIndex;
        }
    }

    public void SetDiscardEvent(int yamaIndex, string eventType, int playerIndex)
    {
        if (!isInputBlocked)
        {
            DiscardedCardYamaIndex = yamaIndex;
            EventType = eventType;
            PlayerIndex = playerIndex;
        }
    }

    public void SetChiEvent(Vector2 chiIndex, string eventType, int playerIndex)
    {
        if (!isInputBlocked)
        {
            ChiIndex = new Vector2(chiIndex.x, chiIndex.y);
            EventType = eventType;
            PlayerIndex = playerIndex;
        }
    }

    public void SetInputBlock(bool active)
    {
        // ���� �� ������ ť�׵� InputAction�� �� ��ü�� �ٲ� �� �ִ�
        // �׷��� �� �� ������ ť�׵��� �� input�� ���� ���� �뵵
        isInputBlocked = active;
    }

    public void Clear()
    {
        DiscardedCardYamaIndex = -1;
        EventType = "";
        PlayerIndex = -1;
        ChiIndex = Vector2.zero;

        SetInputBlock(false);
    }
}
