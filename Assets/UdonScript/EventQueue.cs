
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

public class EventQueue : UdonSharpBehaviour
{
    private InputEvent[] events;
    private int tail = 0;
    private int head = 0;
    private int size = 0;

    public void Initialize()
    {
        events = GetComponentsInChildren<InputEvent>();
        size = events.Length;
    }

    public bool IsQueueEmpty() { return head == tail; }
    public bool IsQueueFull() { return head == (tail + 1) % size; }

    public void SetUIEvent(string uiName, int playerIndex)
    {
        var inputEvent = GetNextInputEvent();

        inputEvent.EventType = uiName;
        inputEvent.PlayerIndex = playerIndex;
    }

    public void SetRegisterPlayerEvent(VRCPlayerApi player)
    {
        var inputEvent = GetNextInputEvent();

        inputEvent.EventType = "Register";
        inputEvent.NewPlayer = player;
    }

    public void SetDiscardEvent(int yamaIndex, int playerIndex)
    {
        var inputEvent = GetNextInputEvent();

        inputEvent.DiscardedCardYamaIndex = yamaIndex;
        inputEvent.EventType = "Discard";
        inputEvent.PlayerIndex = playerIndex;
    }

    public void SetRiichiEvent(int yamaIndex, int playerIndex)
    {
        var inputEvent = GetNextInputEvent();

        inputEvent.DiscardedCardYamaIndex = yamaIndex;
        inputEvent.EventType = "RiichiDiscard";
        inputEvent.PlayerIndex = playerIndex;
    }

    public void SetAutoDiscardEvent(int yamaIndex, int playerIndex)
    {
        var inputEvent = GetNextInputEvent();

        inputEvent.DiscardedCardYamaIndex = yamaIndex;
        inputEvent.EventType = "AutoDiscard";
        inputEvent.PlayerIndex = playerIndex;
    }

    public void SetChiEvent(Vector2 chiIndex, string eventType, int playerIndex)
    {
        var inputEvent = GetNextInputEvent();

        inputEvent.ChiIndex = new Vector2(chiIndex.x, chiIndex.y);
        inputEvent.EventType = eventType;
        inputEvent.PlayerIndex = playerIndex;
    }

    public void AnnounceDraw(string reason)
    {
        var inputEvent = GetNextInputEvent();

        inputEvent.DrawReason = reason;
    }

    InputEvent GetNextInputEvent()
    {
        if (IsQueueFull())
        {
            // 최대 큐잉 가능한 갯수를 넘어서 들어오면?
            // 그냥 맨 마지막 InputEvent를 바꾼다
            // 마우스로 연타해도 Update 속도가 워낙 빨라서 다 채워질 일은 없긴 할 것이다
            Debug.Log("too many input");
            return events[tail];
        }

        tail = GetNextIndex(tail);
        return events[tail];
    }

    public InputEvent Dequeue()
    {
        if (IsQueueEmpty())
        {
            return null;
        }

        head = GetNextIndex(head);
        return events[head];
    }

    int GetNextIndex(int index)
    {
        return (index + 1) % size;
    }
}
