using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ClockManager : MonoBehaviour
{
    public TextMeshProUGUI text;
    public int minuteRandomMax = 10;
    public int minuteRandomMin = 1;
    public GameObject am;
    public GameObject pm;
    GameManager gm;
    ScheduleManager sm;
    int minuteRandom;
    int prevMin;
    int prevHour = -1;

    private void Awake()
    {
        gm = FindFirstObjectByType<GameManager>();
        sm = FindFirstObjectByType<ScheduleManager>();
    }

    private void Start()
    {
        gm.OnTimeChange += ResetMinuteRandom;
        gm.OnNightSkipStart += ResetMinuteRandom;
        gm.OnDayStart += ResetMinuteRandom;
        sm.OnBlockStart += ResetMinuteBlockChange;
        sm.OnBlockEnd += ResetMinuteBlockChange;
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
        if (prevHour !=  hour)
        {
            prevHour = hour;
            minuteRandom = 0;
        }
        if (minute >= minuteRandom)
        {
            minute = Mathf.FloorToInt(minuteProgress * 60f);
            prevMin = minute;
            minuteRandom = minute + Random.Range(minuteRandomMin, minuteRandomMax);
        } 
        else
        {
            minDisplay = prevMin.ToString();
        }
        if (minDisplay.Length == 1) minDisplay = "0" + minDisplay;
        if (hour == 0) hour = 24;
        bool isAM = true;
        if (hour > 11 && hour != 24) isAM = false;
        SetMeridiem(isAM);
        if (hour > 12) hour -= 12;

        text.text = $"{hour}:{minDisplay}";
    }

    void SetMeridiem(bool isAM)
    {
        if (am.activeSelf == isAM) return;
        am.SetActive(isAM);
        pm.SetActive(!isAM);
    }

    void ResetMinuteRandom()
    {
        minuteRandom = 0;
    }

    void ResetMinuteBlockChange(ScheduleBlock to)
    {
        minuteRandom = 0;
    }
}
