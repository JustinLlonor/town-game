using System;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIcon
{
    public string name;
    public Texture2D texture;
    public Vector3 position;
    public float rotation;
    public Vector2 size;
    public bool usesWorldRotation;
    public string hoverText;

    public MinimapIcon(string name, Texture2D texture, Vector3 position, float rotation, Vector2 size, bool usesWorldRotation, string hoverText)
    {
        this.name = name;
        this.texture = texture;
        this.position = position;
        this.rotation = rotation;
        this.size = size;
        this.usesWorldRotation = usesWorldRotation;
        this.hoverText = hoverText;
    }
}