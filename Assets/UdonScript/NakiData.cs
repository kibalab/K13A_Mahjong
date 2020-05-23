
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class NakiData : UdonSharpBehaviour
{
    public string Type;
    public CardComponent[] Cards;
    public void Initialize(string type, CardComponent[] cards)
    {
        Type = type;
        Cards = cards;
    }
}
