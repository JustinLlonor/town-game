using Fusion;
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
    private Dictionary<Vector2Int, GameObject> jobObjects = new Dictionary<Vector2Int, GameObject>();
    // Keeping track of apply objects and order
    private Dictionary<JobApplication, GameObject> appObjects = new Dictionary<JobApplication, GameObject>();
    private List<JobApplication> applyList = new List<JobApplication>();
    PositionManager positionManager;
    ApplicationManager applicationManager;
    GameManager gameManager;
    PositionButtonUI selectedButton;
    PlayerManager playerManager;

    public void Init()
    {
        positionManager = FindAnyObjectByType<PositionManager>();
        applicationManager = FindAnyObjectByType<ApplicationManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        playerManager = FindAnyObjectByType<PlayerManager>();
        applicationManager.onApplicationAdd += AddToApplyList;
        applicationManager.onApplicationRemove += RemoveFromApplyList;
        positionManager.onJobAdd += AddToJobsList;
        positionManager.onJobRemove += RemoveFromJobsList;
    }

    /// <summary>
    /// Adds this job to the job list, list is ordered from oldest to newest
    /// </summary>
    /// <param name="job"></param>
    public void AddToJobsList(Vector2Int jobRef)
    {
        Job ownedJob = positionManager.GetJobFromRef(jobRef);
        int index = jobObjects.Count + 1;
        GameObject newButton = Instantiate(positionButton, positionHolder);
        PositionButtonUI pbui = newButton.GetComponent<PositionButtonUI>();
        pbui.SetColor(ownedJob.handler.jobColor);
        pbui.SetText(ownedJob.name);
        pbui.SetIcon(ownedJob.icon);
        newButton.transform.SetSiblingIndex(index);
        jobObjects.Add(jobRef, newButton);
        pbui.button.onClick.AddListener(delegate
        {
            if (selectedButton == pbui)
            {
                selectedButton = null;
                jobDescriptionUI.ToggleDescription(false);
                return;
            }
            selectedButton = pbui;
            jobDescriptionUI.ToggleDescription(true);
            jobDescriptionUI.ToggleExtraInfo(true);
            jobDescriptionUI.SetTitle(ownedJob.name);
            jobDescriptionUI.UpdatePlayerCount(positionManager.GetJobPlayerCount(jobRef), ownedJob.maxPlayers);
            jobDescriptionUI.SetDescription(ownedJob.description);
            jobDescriptionUI.HideDeadline();
            jobDescriptionUI.SetAccess(ownedJob.buildingAccess);
            jobDescriptionUI.SetPay(ownedJob.pay);
            jobDescriptionUI.SetHours(ownedJob.timeCommitment);
            jobDescriptionUI.ShowButton("RESIGN");
            jobDescriptionUI.button.onClick.RemoveAllListeners();
            // TODO: Add resignation code
        });
    }

    public void RemoveFromJobsList(Vector2Int jobRef)
    {
        Destroy(jobObjects[jobRef].gameObject);
        jobObjects.Remove(jobRef);
    }

    /// <summary>
    /// Adds this job application to the apply list, only if the player is in the same branch
    /// </summary>
    /// <param name="job"></param>
    public void AddToApplyList(JobApplication application)
    {
        PlayerRef localPlayer = FindAnyObjectByType<RunnerManager>().nRunner.LocalPlayer;// do this differently in the future probably
        int localBranch = positionManager.GetBranch(localPlayer);
        if ((application.branchReference == localBranch) ^ (application.jobReference >= 0)) return;
        if (positionManager.PlayerHasJob(localPlayer, application.GetJobRef())) return; // If the player has the job, then don't add the element
        // Adds the button and inserts it in the correct place
        GameObject newButton = Instantiate(positionButton, positionHolder);
        int applyIndex = GetApplyIndex(application);
        applyList.Insert(applyIndex, application);
        newButton.transform.SetSiblingIndex(applyHeader.GetSiblingIndex() + applyIndex + 1);
        appObjects.Add(application, newButton);

        // Button properties
        PositionButtonUI pbui = newButton.GetComponent<PositionButtonUI>();
        Vector2Int appRef = new Vector2Int(application.branchReference, application.jobReference);
        if (application.jobReference >= 0)
        {
            Job appJob = positionManager.GetJobFromRef(appRef);
            pbui.SetColor(appJob.handler.jobColor);
            pbui.SetText(appJob.name);
            pbui.SetIcon(appJob.icon);
            pbui.button.onClick.AddListener(delegate
            {
                if (selectedButton == pbui)
                {
                    selectedButton = null;
                    jobDescriptionUI.ToggleDescription(false);
                    return;
                }
                selectedButton = pbui;
                jobDescriptionUI.ToggleDescription(true);
                jobDescriptionUI.ToggleExtraInfo(true);
                jobDescriptionUI.SetTitle(appJob.name);
                jobDescriptionUI.UpdatePlayerCount(positionManager.GetJobPlayerCount(appRef), appJob.maxPlayers);
                jobDescriptionUI.SetDescription(appJob.description);
                jobDescriptionUI.SetDeadline(gameManager.PeriodToClockString(application.deadline / gameManager.hourLength));
                jobDescriptionUI.SetAccess(appJob.buildingAccess);
                jobDescriptionUI.SetPay(appJob.pay);
                jobDescriptionUI.SetHours(appJob.timeCommitment);
                LoadApplied(pbui, appRef);
            });
        }
        else
        {
            Branch appBranch = positionManager.GetBranchFromIndex(application.branchReference);
            pbui.SetColor(appBranch.color, false);
            pbui.SetText(appBranch.name + " Branch");
            pbui.SetIcon(appBranch.icon);
            pbui.button.onClick.AddListener(delegate
            {
                if (selectedButton == pbui)
                {
                    selectedButton = null;
                    jobDescriptionUI.ToggleDescription(false);
                    return;
                }
                selectedButton = pbui;
                jobDescriptionUI.ToggleDescription(true);
                jobDescriptionUI.SetTitle(appBranch.name + " Branch");
                jobDescriptionUI.UpdatePlayerCount(positionManager.GetBranchPlayerCount(application.branchReference), appBranch.maxPlayers);
                jobDescriptionUI.SetDescription(appBranch.description);
                jobDescriptionUI.SetDeadline(gameManager.PeriodToClockString(application.deadline / gameManager.hourLength));
                jobDescriptionUI.ToggleExtraInfo(false);
                LoadApplied(pbui, appRef);
            });
        }
    }

    private void SetApplicationDelegate(Vector2Int appRef, PositionButtonUI pbui) {
        jobDescriptionUI.button.onClick.RemoveAllListeners();
        if (playerManager.currentPlayer != null)
        {
            Player player = playerManager.currentPlayer.GetComponent<Player>();
            jobDescriptionUI.button.onClick.AddListener(delegate { 
                player.RPC_SubmitApplication(appRef); 
                jobDescriptionUI.ShowErrorText("Successfully applied", Color.white);
                pbui.applied = true;
            });
        }
    }

    private void LoadApplied(PositionButtonUI pbui, Vector2Int appRef)
    {
        if (!pbui.applied)
        {
            jobDescriptionUI.ShowButton("APPLY");
            SetApplicationDelegate(appRef, pbui);
            return;
        }
        jobDescriptionUI.ShowErrorText("Successfully applied", Color.white);
    }

    public void RemoveFromApplyList(JobApplication application)
    {
        applyList.Remove(application);
        if (appObjects.ContainsKey(application))
        {
            PositionButtonUI pbui = appObjects[application].GetComponent<PositionButtonUI>();
            if (selectedButton == pbui)
            {
                jobDescriptionUI.ToggleDescription(false);
            }
            Destroy(appObjects[application]);
        }
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
