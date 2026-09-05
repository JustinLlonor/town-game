using Fusion;
using UnityEngine;

[System.Serializable]
public struct Evidence
{
    public string[] icons;
    // Descriptions with higher indexes are revealed to players with higher inspect power
    public string[] descriptions;
    public float time;

    public Evidence(string[] icons, string[] descriptions, float time)
    {
        this.icons = icons;
        this.descriptions = descriptions;
        this.time = time;
    }
}