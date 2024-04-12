using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    private bool _mouseButtonDown0;
    private bool _mouseButtonDown1;

    private bool _mouseButton0;
    private bool _mouseButton1;

    private bool _buttonE;
    private bool _buttonEDown;
    private bool _buttonF;
    private bool _buttonQ;
    private bool _buttonR;
    private bool _buttonZ;
    private bool _buttonX;
    private bool _buttonCapsLock;
    private bool _buttonShift;
    private bool _buttonJump;

    private float _mouseScrollY = 0;
    private Vector2 _mouseMovement = Vector2.zero;   

    void Update()
    {
        CollectInput();
    }

    public InputData GetPlayerInput()
    {
        InputData data = new InputData();

        data.direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        data.mouseMovement = _mouseMovement;
        _mouseMovement = Vector2.zero;

        data.mouseScrollY = _mouseScrollY;
        _mouseScrollY = 0;

        if (_mouseButtonDown0)
            data.buttons |= InputData.MOUSEBUTTON1;
        _mouseButtonDown0 = false;

        if (_mouseButtonDown1)
            data.buttons |= InputData.MOUSEBUTTON2;
        _mouseButtonDown1 = false;

        data.mouseButton0 = _mouseButton0;
        _mouseButton0 = false;

        data.mouseButton1 = _mouseButton1;
        _mouseButton1 = false;

        data.buttonE = _buttonE;
        _buttonE = false;

        data.buttonF = _buttonF;
        _buttonF = false;

        data.buttonQ = _buttonQ;
        _buttonQ = false;

        data.buttonR = _buttonR;
        _buttonR = false;

        data.buttonZ = _buttonZ;
        _buttonZ = false;

        data.buttonX = _buttonX;
        _buttonX = false;

        data.buttonShift = _buttonShift;
        _buttonShift = false;

        data.buttonCapsLock = _buttonCapsLock;
        _buttonCapsLock = false;

        data.buttonJump = _buttonJump;
        _buttonJump = false;

        data.buttonEDown = _buttonEDown;
        _buttonEDown = false;

        return data;
    }
    private void CollectInput()
    {
        _mouseButtonDown0 = _mouseButtonDown0 | Input.GetMouseButtonDown(0);
        _mouseButtonDown1 = _mouseButtonDown1 | Input.GetMouseButtonDown(1);

        _mouseButton0 = _mouseButton0 | Input.GetMouseButton(0);
        _mouseButton1 = _mouseButton1 | Input.GetMouseButton(1);

        _buttonE = _buttonE | Input.GetKey(KeyCode.E);
        _buttonEDown = _buttonEDown | Input.GetKeyDown(KeyCode.E);
        _buttonF = _buttonF | Input.GetKeyDown(KeyCode.F);
        _buttonQ = _buttonQ | Input.GetKey(KeyCode.Q);
        _buttonR = _buttonR | Input.GetKeyDown(KeyCode.R);
        _buttonZ = _buttonZ | Input.GetKeyDown(KeyCode.Z);
        _buttonX = _buttonX | Input.GetKeyDown(KeyCode.X);
        _buttonCapsLock = _buttonCapsLock | Input.GetKey(KeyCode.CapsLock);
        _buttonShift = _buttonShift | Input.GetKey(KeyCode.LeftShift);
        _buttonJump = _buttonJump | Input.GetKeyDown(KeyCode.Space);

        _mouseMovement += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        _mouseScrollY += Input.mouseScrollDelta.y;
    }
}
