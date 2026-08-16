using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task", menuName = "Tasks/Task")]
public class DynamicTask : ScriptableObject
{
    public string displayName;
    [Tooltip("The max amount of players that can have this task")]
    public int playerLimit = 1;
    [Tooltip("The lowest level that can be assigned to this task")]
    public int level;
    [Tooltip("A list of subtasks. A subtask can only be active once every other subtask below it is complete")]
    public Subtask[] subtasks;

}
