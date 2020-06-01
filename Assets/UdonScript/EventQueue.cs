
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventQueue : UdonSharpBehaviour
{
    private InputEvent[] events;
    private int count = 0;

    private void Start()
    {
        events = new InputEvent[256];
    }

    public bool IsQueueEmpty() { return count == 0; }

    public void Enqueue(InputEvent e)
    {
        events[count++] = e;
    }

    public InputEvent Dequeue()
    {
        InputEvent tmp = events[0];
        events[0] = null;
        for (var i = 1; i < count; i++)
        {
            events[i - 1] = events[i];
        }
        count--;
        return tmp;
    }
}
