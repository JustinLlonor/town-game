using System.Collections;
using System.Collections.Generic;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public const byte jumpButton = 1;

    public NetworkButtons buttons;
    public float camDirection;
    public float camDirectionX;
    public Vector2Compressed direction;
}
