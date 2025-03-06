using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TearoutPhys : MonoBehaviour
{
    public GameObject taskPrefab;
    public Transform subtextHolder;

    public void AddUITask(string name, bool completed)
    {
        GameObject newTask = Instantiate(taskPrefab, subtextHolder);
        newTask.GetComponent<TextMeshProUGUI>().text = name;
        newTask.transform.GetChild(0).gameObject.SetActive(completed); // set the x
    }

    public void SetUITaskCompleted(int blockIndex, bool completed)
    {
        subtextHolder.GetChild(blockIndex).GetChild(0).gameObject.SetActive(completed);
    }

    public void ClearUITasks()
    {
        foreach (Transform child in subtextHolder)
        {
            Destroy(child);
        }
    }

    public void StartTaskReveal(GameObject minimapHolder) // Add to delegate?
    {

    }
}
