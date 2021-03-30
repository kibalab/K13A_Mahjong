
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ResetButton : UdonSharpBehaviour
{
    public TableManager TB;
    public override void Interact()
    {
        TB.resetTable();
    }
}
