using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeGrid : MonoBehaviour
{
    public GameObject timeBlockPrefab;
    public TabSchedule ts;
    public int periodAmount = 14;
    public float startTime = 7f;
    bool gridCreated = false;

    private void OnEnable()
    {
        if (gridCreated) return;
        gridCreated = true;
        GameManager gm = FindFirstObjectByType<GameManager>();
        for (int i = 0; i < periodAmount+1; i ++)
        {
            string periodText = "";
            if (i % 2 == 0) periodText = gm.PeriodToClockString(startTime + i);
            GameObject tbp = Instantiate(timeBlockPrefab, transform);
            ((RectTransform)tbp.transform).sizeDelta = new Vector2(ts.maxWidth, ts.hourHeight);
            tbp.GetComponent<PhysTimeBlock>().ChangeTimeText(periodText);
            if (!(i % 2 == 0)) tbp.GetComponent<PhysTimeBlock>().ChangeTimeText(periodText);
        }
    }
}
