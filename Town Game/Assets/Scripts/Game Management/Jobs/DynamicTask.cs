using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Task", menuName = "Tasks/Task")]
public class DynamicTask : ScriptableObject
{
    public string displayName;
    [Tooltip("A list of subtasks. A subtask can only be active once every other subtask below it is complete")]
    public Subtask[] subtasks;

}
