using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionUI : MonoBehaviour
{
    public Transform positionHolder;
    public Transform jobHeader;
    public Transform applyHeader;
    public GameObject positionButton;
    public JobDescUI jobDescriptionUI;
    // Keeping track of job objects
    private Dictionary<JobApplication, GameObject> jobObjects = new Dictionary<JobApplication, GameObject>();
    // Keeping track of apply objects and order
    private Dictionary<JobApplication, GameObject> appObjects = new Dictionary<JobApplication, GameObject>();
    private List<JobApplication> applyList = new List<JobApplication>();
    PositionManager positionManager;
    ApplicationManager applicationManager;

    // TODO: Make Init get called on start
    public void Init()
    {
        positionManager = FindAnyObjectByType<PositionManager>();
        applicationManager = FindAnyObjectByType<ApplicationManager>();
        applicationManager.onApplicationAdd += AddToApplyList;
        applicationManager.onApplicationRemove += RemoveFromApplyList;
    }

    /// <summary>
    /// Adds this job to the job list, list is ordered from oldest to newest
    /// </summary>
    /// <param name="job"></param>
    public void AddToJobsList(Vector2Int jobRef)
    {
        
    }

    public void RemoveFromJobsList(Vector2Int jobRef)
    {

    }

    /// <summary>
    /// Adds this job application to the apply list, only if the player is in the same branch
    /// </summary>
    /// <param name="job"></param>
    public void AddToApplyList(JobApplication application)
    {
        // Adds the button and inserts it in the correct place
        int applyIndex = GetApplyIndex(application);
        applyList.Insert(applyIndex, application);
        GameObject newButton = Instantiate(positionButton, positionHolder);
        newButton.transform.SetSiblingIndex(jobObjects.Count + 1 + applyIndex);
        appObjects.Add(application, newButton);

        // Button properties
        PositionButtonUI pbui = newButton.GetComponent<PositionButtonUI>();
        if (application.jobReference >= 0)
        {
            Job appJob = positionManager.GetJobFromRef(new Vector2Int(application.branchReference, application.jobReference));
            pbui.SetColor(appJob.handler.jobColor);
            pbui.SetText(appJob.name);
            pbui.SetIcon(appJob.icon);
        }
        else
        {
            Branch appBranch = positionManager.GetBranchFromIndex(application.branchReference);
            pbui.SetColor(appBranch.color);
            pbui.SetText(appBranch.name);
            pbui.SetIcon(appBranch.icon);
        }
    }

    public void RemoveFromApplyList(JobApplication application)
    {
        applyList.Remove(application);
        Destroy(appObjects[application]);
        appObjects.Remove(application);
    }

    /// <summary>
    /// Finds the index to insert this job application. Sorts by priority, least employed jobs to most employed
    /// </summary>
    /// <param name="application"></param>
    /// <returns></returns>
    private int GetApplyIndex(JobApplication application) {
        if (application.jobReference < 0) return applyList.Count;
        // Gets the player count of the to-be-inserted application
        Job appJob = positionManager.GetJobFromRef(new Vector2Int(application.branchReference, application.jobReference));
        int appPlayerCount = appJob.assignedPlayers.Count;
        // Finds an index to insert the application into
        int i = 0;
        for (i = 0; i < applyList.Count; i++)
        {
            // Append right before the branch application
            if (applyList[i].jobReference < 0) break;
            Job compJob = positionManager.GetJobFromRef(new Vector2Int(applyList[i].branchReference, applyList[i].jobReference));
            int compPlayerCount = compJob.assignedPlayers.Count;
            // If greater, continue, the moment we are less than or equal to something, append right there
            if (appPlayerCount > compPlayerCount) continue;
            break;
        }
        return i;
    }
}
