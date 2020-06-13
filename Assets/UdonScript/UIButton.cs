
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class UIButton : UdonSharpBehaviour
{
    [SerializeField] public UIManager UIManager;

    public override void Interact()
    {
        UIManager.OnClick(gameObject.name);
    }
}
