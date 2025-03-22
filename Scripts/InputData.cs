using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputData
{
    public const byte MOUSEBUTTON1 = 0x01;
    public const byte MOUSEBUTTON2 = 0x02;

    public bool buttonE;
    public bool buttonEDown;
    public bool buttonF;
    public bool buttonQ;
    public bool buttonR;
    public bool buttonH;
    public bool buttonL;
    public bool buttonJump;

    public bool buttonZ;
    public bool buttonX;

    public bool buttonCapsLock;
    public bool buttonShift;

    public bool mouseButton0;
    public bool mouseButton1;

    public byte buttons;
    public Vector3 direction;
    public Vector2 mouseMovement;
    public float mouseScrollY;
}
