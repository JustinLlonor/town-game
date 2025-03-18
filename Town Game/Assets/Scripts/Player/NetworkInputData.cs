using System.Collections;
using System.Collections.Generic;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public enum Buttons
    {
        Jump = 0,
        Crouch = 1,
        Sprint = 2,
        Drop = 3,
        ExitObserve = 4,
        PrimaryItem = 5,
        SecondaryItem = 6
    }

    public NetworkButtons buttons;
    public float camDirection;
    public float camDirectionX;
    public Vector2Compressed direction;
    public int hotbarKey;

    public NetworkBool menu;
    public NetworkBool interactPressed;
    public int interaction;

    public int subInteractableIndex; // -1 means not pressed
}
