
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventQueue : UdonSharpBehaviour
{
    public CardComponent[] components;
    public int Count = 0;


    private void Start()
    {
         components = new CardComponent[256];
    }

    public bool IsQueueEmpty() { return Count == 0; }

    public void Enqueue(CardComponent component) 
    {
        components[Count++] = component;
    }

    public CardComponent Dequeue() 
    {
        Debug.Log(Count);
        CardComponent tmp = components[0];
        components[0] = null;
        for (var i = 1; i < Count; i++)
        {
            components[i-1] = components[i - 1];
        }
        Count--;
        return tmp;
    }
}
