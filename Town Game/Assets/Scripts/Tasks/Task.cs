using Fusion;
using System;
using System.Collections.Generic;

[System.Serializable]
public struct Task : INetworkStruct, IEquatable<Task>
{
    public NetworkString<_256> name;
    public int category;
    public float secondsTaken;
    public NetworkString<_64> room;
    public NetworkBool isCompleted;

    public Task(NetworkString<_256> name, int category, float secondsTaken, NetworkString<_64> room, NetworkBool isCompleted)
    {
        this.name = name;
        this.category = category;
        this.secondsTaken = secondsTaken;
        this.room = room;
        this.isCompleted = isCompleted;
    }

    public override bool Equals(object obj)
    {
        return obj is Task task && Equals(task);
    }

    public bool Equals(Task other)
    {
        return name.Equals(other.name) &&
               category == other.category &&
               secondsTaken == other.secondsTaken &&
               room.Equals(other.room) &&
               isCompleted.Equals(other.isCompleted);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(name, category, secondsTaken, room, isCompleted);
    }

    public static bool operator ==(Task left, Task right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Task left, Task right)
    {
        return !(left == right);
    }

    public static Task None
    {
        get
        {
            return new Task("None", -999, -1f, "", false);
        }
    }
}
