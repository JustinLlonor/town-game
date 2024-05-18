using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Holds tasks for rooms and states
public class TaskHolder : MonoBehaviour
{
    public List<Task> tasks = new List<Task>();
    public TasksUpdate OnTasksUpdate;
    public bool assigned = false;
    public delegate void TasksUpdate();

    public void SetTaskProgression(string taskName, float progress)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].name == taskName)
            {
                tasks[i].progress = progress;
                OnTasksUpdate?.Invoke();
            }
        }
    }

    public void CreateTask(string taskName, Task.Type type, float progress = 0f)
    {
        Task foundTask = tasks.FirstOrDefault(i => i.name == taskName);
        if (foundTask != null) return;
        tasks.Add(new Task(taskName, type, progress));
        OnTasksUpdate?.Invoke();
    }

    public void RemoveTask(string taskName)
    {
        for (int i = 0; i < tasks.Count; i++)
        {
            if (tasks[i].name == taskName)
            {
                tasks.RemoveAt(i);
                OnTasksUpdate?.Invoke();
                break;
            }
        }
    }
}
