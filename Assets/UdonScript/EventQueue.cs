
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EventQueue : UdonSharpBehaviour
{
    public GameManager gameManager;
    public KList events;
    

    private void Start()
    {
    }

    public void Enqueue(CardComponent component) 
    {
        events.Add(component);
    }

    public CardComponent Dequeue() 
    {
        return (CardComponent)events.RemoveAt(0);
    }

    public bool IsQueueEmpty() 
    {
        bool IsEmpty;
        if(events.Count <= 0)
        {
            return true;
            IsEmpty = true;
        } else
        {
            return false;
            IsEmpty = false;
        }
        return IsEmpty;
    }
}
