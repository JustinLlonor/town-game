using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public int minuteRandomMax = 10;
    public int minuteRandomMin = 1;
    GameManager gm;
    ScheduleManager sm;
    int minuteRandom;
    int prevMin;
    int prevHour = -1;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        sm = FindObjectOfType<ScheduleManager>();
    }

    private void Start()
    {
        gm.OnTimeChange += ResetMinuteRandom;
        sm.OnBlockChange += ResetMinuteBlockChange;
    }

    private void Update()
    {
        if (gm == null) return;
        UpdateTime();
    }

    void UpdateTime()
    {
        // number from 0 to 1 of the day's progress
        float dayProgress = gm.GetDayProgress();
        //float dayProgress = (gm.currentPeriod - (gm.currentDay * 24f)) / 24f;
        int hour = Mathf.FloorToInt(dayProgress * 24) + 1;
        // number from 0 to 1 of the minute percentage
        float minuteProgress = dayProgress * 24 - hour + 1;
        int minute = Mathf.FloorToInt(minuteProgress * 60f);
        string minDisplay = minute.ToString();
        if (minute >= minuteRandom)
        {
            minute = Mathf.FloorToInt(minuteProgress * 60f);
            prevMin = minute;
            minuteRandom = minute + Random.Range(minuteRandomMin, minuteRandomMax);
        } else
        {
            minDisplay = prevMin.ToString();
        }
        if (minDisplay.Length == 1) minDisplay = "0" + minDisplay;
        if (hour == 0) hour = 24;
        string meridiem = "AM";
        if (hour > 11 && hour != 24) meridiem = "PM";
        if (hour > 12) hour -= 12;

        text.text = $"{hour}:{minDisplay} {meridiem}";
    }

    void ResetMinuteRandom()
    {
        minuteRandom = 0;
    }

    void ResetMinuteBlockChange(ScheduleBlock from, ScheduleBlock to)
    {
        minuteRandom = 0;
    }
}
