using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TearoutPhys : MonoBehaviour
{
    public GameObject taskPrefab;
    public Transform subtextHolder;
    public float padding = 20.4f;
    public VerticalLayoutGroup layoutGroup;

    public void AddUITask(string name, bool completed)
    {
        GameObject newTask = Instantiate(taskPrefab, subtextHolder);
        newTask.GetComponent<TextMeshProUGUI>().text = name;
        newTask.transform.GetChild(0).gameObject.SetActive(completed); // set the x
    }

    public void SetUITaskCompleted(int blockIndex, bool completed)
    {
        Debug.Log("Setting");
        Transform completedT = subtextHolder.GetChild(blockIndex);
        completedT.gameObject.GetComponent<Animator>().Play("TaskComplete");
        completedT.GetChild(0).gameObject.SetActive(completed);
    }

    public void ClearUITasks()
    {
        foreach (Transform child in subtextHolder)
        {
            Destroy(child);
        }
    }

    public float GetSubtextHeight()
    {
        Canvas.ForceUpdateCanvases();
        float totalHeight = 0f;
        foreach (Transform child in subtextHolder)
        {
            totalHeight += ((RectTransform)child).sizeDelta.y * ((RectTransform)subtextHolder).localScale.y;
            Debug.Log("new total: " + totalHeight);
        }
        return totalHeight;
    }
}
