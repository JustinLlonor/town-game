using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScheduleUI : MonoBehaviour
{
    public GameObject scheduleBlockPrefab;

    public void AddScheduleBlock()
    {

    }

    public void ClearScheduleBlocks()
    {
        foreach (Transform child in transform)
        {
            Destroy(child);
        }
    }
}
