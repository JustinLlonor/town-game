using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabUI : MonoBehaviour
{
    public UIPlayerList playerList;
    public TabSchedule tabSchedule;

    private void Awake()
    {
        if (tabSchedule != null)
        {
            //playerList.OnClickPlayer += tabSchedule.ResetReadDay;
            //playerList.OnClickPlayer += tabSchedule.DisplaySchedule;
            //playerList.OnDeselectPlayer += tabSchedule.DeselectSchedule;
        }
    }

    public void UpdatePlayerList()
    {
        playerList.UpdatePlayerList();
    }
}
