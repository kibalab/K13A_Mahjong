
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
        // 여러 번 누르면 큐잉된 InputAction의 값 자체가 바뀔 수 있다
        // 그래서 한 번 눌러서 큐잉됐을 때 input을 막기 위한 용도
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
