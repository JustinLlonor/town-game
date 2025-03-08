using Fusion;
using System.Collections.Generic;

[System.Serializable]
public class Task
{
    public static int idIndex = 0;

    public string name;
    public int category;
    public float secondsTaken;
    public string room;
    public bool isCompleted;
    public int id;

    /// <summary>
    /// Constructor for a task. If the id parameter is left alone, will automatically increment task id index
    /// </summary>
    /// <param name="name"></param>
    /// <param name="category"></param>
    /// <param name="secondsTaken"></param>
    /// <param name="id"></param>
    public Task(string name, int category, float secondsTaken, string room, bool isCompleted = false, int id = -1)
    {
        this.name = name;
        this.category = category;
        this.secondsTaken = secondsTaken;
        this.room = room;
        this.isCompleted = isCompleted;
        if (id != -1)
        {
            this.id = idIndex++;
        }
        else
        {
            this.id = id;
        }
    }
}
