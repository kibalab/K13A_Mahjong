
using UdonSharp;
using UnityEngine;

public class EventQueue : UdonSharpBehaviour
{
    private InputEvent[] events;
    private int currIndex = 0;
    private int topIndex = 0;

    public void Initialize()
    {
        events = GetComponentsInChildren<InputEvent>();
    }

    public bool IsQueueEmpty() { return topIndex == 0; }

    public void SetUIEvent(string uiName, int playerIndex)
    {
        var inputEvent = GetInputEvent();

        inputEvent.EventType = uiName;
        inputEvent.PlayerIndex = playerIndex;
    }

    public void SetDiscardEvent(int yamaIndex, string eventType, int playerIndex)
    {
        var inputEvent = GetInputEvent();

        inputEvent.DiscardedCardYamaIndex = yamaIndex;
        inputEvent.EventType = eventType;
        inputEvent.PlayerIndex = playerIndex;
    }

    public void SetChiEvent(Vector2 chiIndex, string eventType, int playerIndex)
    {
        var inputEvent = GetInputEvent();

        inputEvent.ChiIndex = new Vector2(chiIndex.x, chiIndex.y);
        inputEvent.EventType = eventType;
        inputEvent.PlayerIndex = playerIndex;
    }

    InputEvent GetInputEvent()
    {
        var inputEvent = events[topIndex];

        if (topIndex + 1 != events.Length)
        {
            ++topIndex;
        }
        else
        {
            Debug.Log("too many input");
        }

        return inputEvent;
    }

    public InputEvent Dequeue()
    {
        var inputEvent = events[currIndex++];

        if (currIndex == topIndex)
        {
            currIndex = 0;
            topIndex = 0;
        }

        return inputEvent;
    }
}
