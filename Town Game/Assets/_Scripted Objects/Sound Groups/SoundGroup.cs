using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sound Group", menuName = "Sound Group")]
public class SoundGroup : ScriptableObject
{
    [Range(0f, 1f)]
    public float volumeMultiplier = 1f;
    public SoundManager.Sound[] sounds;
}
