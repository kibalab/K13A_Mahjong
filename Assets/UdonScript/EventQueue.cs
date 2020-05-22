
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventQueue : UdonSharpBehaviour
{
    private CardComponent[] components;
    private int count = 0;


    private void Start()
    {
         components = new CardComponent[256];
    }

    public bool IsQueueEmpty() { return count == 0; }

    public void Enqueue(CardComponent component) 
    {
        components[count++] = component;
    }

    public CardComponent Dequeue() 
    {
        Debug.Log(count);
        CardComponent tmp = components[0];
        components[0] = null;
        for (var i = 1; i < count; i++)
        {
            components[i-1] = components[i];
        }
        count--;
        return tmp;
    }
}
