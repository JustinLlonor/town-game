using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Task
{
    public string name;
    public Type type;
    public float progress;

    public enum Type
    {
        Completion = 0,
        Maintainence = 1
    }

    public Task(string name, Type type, float progress)
    {
        this.name = name;
        this.type = type;
        this.progress = progress;
    }
}
