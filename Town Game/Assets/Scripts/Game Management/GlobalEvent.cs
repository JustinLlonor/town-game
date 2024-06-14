using System.Collections;
using System.Collections.Generic;
using System.Drawing;

[System.Serializable]
public class GlobalEvent
{
    public string name;
    public float time;
    public float length;
    public bool cultistEvent;

    public GlobalEvent(string name, float time, float length, bool cultistEvent)
    {
        this.name = name;
        this.time = time;
        this.length = length;
        this.cultistEvent = cultistEvent;
    }
}