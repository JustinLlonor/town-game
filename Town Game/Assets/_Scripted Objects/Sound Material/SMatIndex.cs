using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "New Sound Index", menuName = "SIndex")]
public class SMatIndex : ScriptableObject
{
    public SoundColor[] soundMaterials = new SoundColor[] { };

    [System.Serializable]
    public class SoundColor
    {
        public Color color;
        public string materialChar;
    }
}
