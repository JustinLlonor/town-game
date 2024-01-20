using UnityEngine;

[System.Serializable]
public class Evidence
{
    public string[] icons;
    // Descriptions with higher indexes are revealed to players with higher inspect power
    public string[] descriptions;
    public float time = -1f;

    public Evidence(string[] icons, string[] descriptions, float time)
    {
        this.icons = icons;
        this.descriptions = descriptions;
        this.time = time;
    }
}