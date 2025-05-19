using UnityEngine;

public class MinimapPointer
{
    public string name;
    public Vector3 position;
    public Color color;
    /// <summary>
    /// If this pointer disappears when it gets within radius
    /// </summary>
    public bool disappearOnSight = false;

    public MinimapPointer(string name, Vector3 position, Color color, bool disappearOnSight)
    {
        this.name = name;
        this.position = position;
        this.color = color;
        this.disappearOnSight = disappearOnSight;
    }
}
