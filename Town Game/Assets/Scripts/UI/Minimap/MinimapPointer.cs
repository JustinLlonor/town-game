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
}
