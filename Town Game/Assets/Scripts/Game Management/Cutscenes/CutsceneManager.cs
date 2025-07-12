using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneManager : NetworkBehaviour
{
    [Networked] public float cutsceneProgress { get; set; } = -1f;
}
