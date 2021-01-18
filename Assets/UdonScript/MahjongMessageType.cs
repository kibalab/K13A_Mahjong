
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MahjongMessageType : UdonSharpBehaviour
{
    public byte RIICHI = 0x01;
    public byte DISPLAY_NAME = 0x02;
    public byte DISCARD = 0x03;
}
