﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;

public class Card : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int GlobalIndex;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;

    [UdonSynced(UdonSyncMode.None)] public Vector3 position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion rotation;
    [UdonSynced(UdonSyncMode.None)] public int Index;

    /*LinkedInInspector*/ public InputEvent InputEvent;
    /*LinkedInInspector*/ public UIManager UIManager;

    private EventQueue eventQueue;
    private BoxCollider boxCollider;

    public override void Interact()
    {
        // SendCustomNetworkEvent(NetworkEventTarget.Owner, "_Interact");
        _Interact();
    }

    public void _Interact()
    {
        InputEvent.Set(Index, "Discard", -1);
        eventQueue.Enqueue(InputEvent);
    }

    public void Initialize(string type, int cardNumber, bool isDora, EventQueue e, CardSprites sprites, HandUtil util)
    {
        eventQueue = e;
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;

        GlobalIndex = util.CardComponentToIndex(type, cardNumber);
        boxCollider = this.GetComponent<BoxCollider>();

        var spriteName = GetCardSpriteName();
        var sprite = sprites.FindSprite(spriteName);
        SetSprite(sprite);
    }

    public BoxCollider SetColliderActivate(bool t)
    {
        boxCollider.enabled = t;
        return boxCollider;
    }

    public void SetSprite(Sprite sprite)
    {
        var display = transform.Find("Display");
        var renderer = display.GetComponent<SpriteRenderer>();

        renderer.sprite = sprite;
    }

    public void SetPosition(Vector3 p, Quaternion r)
    {
        position = p;
        rotation = r;

        //SendCustomNetworkEvent(NetworkEventTarget.All, "_SetPosition");
        _SetPosition();
    }

    public void _SetPosition()
    {
        transform.SetPositionAndRotation(position, rotation);
    }

    public string GetCardSpriteName()
    {
        switch (Type)
        {
            case "동":
            case "남":
            case "서":
            case "북":
            case "백":
            case "발":
            case "중":
                return Type;
            default:
                return Type + CardNumber + (IsDora ? "도라" : "");
        }
    }
}
