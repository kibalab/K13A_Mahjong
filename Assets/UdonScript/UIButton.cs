
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIButton : UdonSharpBehaviour
{
    public string buttonName;

    public UIManager uiManager;

    public void Initialize(UIManager uiManager)
    {
        this.uiManager = uiManager;

        buttonName = this.gameObject.name;
    }

    public override void Interact()
    {
        uiManager._ButtonEventInterface(buttonName);
    }
}
