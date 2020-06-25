﻿using UnityEngine;
using VRC.Udon.Common.Interfaces;
using UdonSharp;
using VRC.SDKBase;
using UnityEngine.SocialPlatforms;

public class Card : UdonSharpBehaviour
{
    [UdonSynced(UdonSyncMode.None)] public string Type;
    [UdonSynced(UdonSyncMode.None)] public int CardNumber;
    [UdonSynced(UdonSyncMode.None)] public int GlobalOrder;
    [UdonSynced(UdonSyncMode.None)] public bool IsDora;
    [UdonSynced(UdonSyncMode.None)] public Vector3 Position;
    [UdonSynced(UdonSyncMode.None)] public Quaternion Rotation;
    [UdonSynced(UdonSyncMode.None)] public bool IsColliderActive;

    public bool IsRinShan;
    public int YamaIndex;
    public int PlayerIndex;

    [SerializeField] private HandUtil HandUtil;
    [SerializeField] private CardSprites CardSprites;
    [SerializeField] private SpriteRenderer SpriteRenderer;
    [SerializeField] private BoxCollider BoxColider;
    [SerializeField] private LogViewer LogViewer;
    [SerializeField] private EventQueue EventQueue;

    private InputEvent inputEvent;
    private bool isSpriteInitialized = false;

    public override void Interact()
    {
        if (Networking.LocalPlayer == null)
        {
            _Interact();
        }
        else
        {
            SendCustomNetworkEvent(NetworkEventTarget.Owner, nameof(_Interact));
        }
    }
    public void _Interact()
    {
        inputEvent.SetDiscardEvent(YamaIndex, "Discard", PlayerIndex);
        EventQueue.Enqueue(inputEvent);
    }

    public void Initialize_Master(string type, int cardNumber, bool isDora)
    {
        Type = type;
        CardNumber = cardNumber;
        IsDora = isDora;
        GlobalOrder = HandUtil.GetGlobalOrder(type, cardNumber);
    }

    public void SetOwnership(int playerIndex, InputEvent inputEvent)
    {
        this.inputEvent = inputEvent;
        this.PlayerIndex = playerIndex;
    }

    public void SetColliderActivate(bool t)
    {
        IsColliderActive = t;
    }

    public void SetPosition(Vector3 position, Quaternion rotation)
    {
        Position = position;
        Rotation = rotation;
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

    private void Update()
    {
        if (!isSpriteInitialized)
        {
            TryInitializeSprite();
        }

        if (BoxColider != null)
        {
            BoxColider.enabled = IsColliderActive;
        }

        transform.SetPositionAndRotation(Position, Rotation);
    }

    void TryInitializeSprite()
    {
        if (SpriteRenderer != null)
        {
            var spriteName = GetCardSpriteName();
            var sprite = CardSprites.FindSprite(spriteName);

            if (sprite != null)
            {
                SpriteRenderer.sprite = sprite;
                isSpriteInitialized = true;
            }
        }
    }
}
