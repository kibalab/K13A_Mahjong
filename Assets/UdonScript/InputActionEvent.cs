
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class InputActionEvent : UdonSharpBehaviour
{
    public CardComponent card;
    public string eventType;
    public VRCPlayerApi author;

    public void Initialize() {  }

}
