using UnityEngine;

[System.Serializable]
public class Evidence
{
    public string name;
    public Texture2D[] icons;
    // Descriptions with higher indexes are revealed to players with higher inspect power
    public string[] descriptions;
    public float time = -1f;
}