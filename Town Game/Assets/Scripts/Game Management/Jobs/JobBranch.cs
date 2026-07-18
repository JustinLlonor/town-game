using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JobBranch
{
    public string name;
    public string description;
    public Texture icon;
    public Color color;
    [Tooltip("The name of the category of rooms involved with this branch")]
    public RoomCategory category;
    [Tooltip("The name of each position within the branch. The index of each name correspond with the index of each position, with 0 being the highest priority position")]
    public string[] positionNames = new string[] { };
    [Tooltip("The max amount of players per each position. -1 means there are infinitely many players")]
    public string[] positionLimits = new string[] { };
    public JobHandler jobHandler;
}
