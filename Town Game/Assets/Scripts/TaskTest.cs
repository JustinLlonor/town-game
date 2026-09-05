using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskTest : MonoBehaviour
{
    public JobHandler jobHandler;
    int currentTaskId;

    private void Awake()
    {
        jobHandler.OnTasksFinishServer += TaskFinish;
    }

    /**
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0)) currentTaskId = jobHandler.AddTask("Do the dishes by <deadline>. No hard feelings if you can't though UWU! Anyway, the weather's pretty bad outside i hate how hot it is.", FindAnyObjectByType<GameManager>().currentPeriod + 3f, Vector3.zero);
        if (Input.GetKeyDown(KeyCode.Alpha9)) jobHandler.CompleteTask(currentTaskId);
        if (Input.GetKeyDown(KeyCode.Alpha8)) jobHandler.CancelTask(currentTaskId);
    }
    **/

    public TaskFinishInfo TaskFinish(List<Task> tasks, PlayerRef player, JobHandler source)
    {
        return new TaskFinishInfo(100f, 0);
    }
}
