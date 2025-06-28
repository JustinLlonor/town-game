using Fusion;
using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Task : INetworkStruct, IEquatable<Task>
{
    public static int idCounter = 0;

    public NetworkString<_256> name;
    public Vector3 location;
    public NetworkBool isCompleted;
    public int id;

    /// <summary>
    /// Creates a task. If the id parameter is set, creates the task with that id.
    /// Otherwise, creates a task with a new id
    /// </summary>
    /// <param name="name"></param>
    /// <param name="room"></param>
    /// <param name="isCompleted"></param>
    /// <param name="id"></param>
    public Task(NetworkString<_256> name, Vector3 location, NetworkBool isCompleted, int id = -1)
    {
        this.name = name;
        this.location = location;
        this.isCompleted = isCompleted;
        if (id >= 0)
        {
            this.id = id;
            return;
        }
        this.id = idCounter;
        idCounter++;
    }

    public static Task None
    {
        get
        {
            return new Task("None", Vector3.zero, false, -999);
        }
    }

    public override bool Equals(object obj)
    {
        return obj is Task task && Equals(task);
    }

    public bool Equals(Task other)
    {
        return name.Equals(other.name) &&
               location.Equals(other.location) &&
               isCompleted.Equals(other.isCompleted) &&
               id == other.id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(name, location, isCompleted, id);
    }

    public static bool operator ==(Task left, Task right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Task left, Task right)
    {
        return !(left == right);
    }
}
