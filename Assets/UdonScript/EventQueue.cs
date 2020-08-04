
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
            // �ִ� ť�� ������ ������ �Ѿ ������?
            // �׳� �� ������ InputEvent�� �ٲ۴�
            // ���콺�� ��Ÿ�ص� Update �ӵ��� ���� ���� �� ä���� ���� ���� �� ���̴�
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
