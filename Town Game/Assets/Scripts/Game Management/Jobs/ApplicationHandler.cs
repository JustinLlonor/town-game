using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Handles applications for every job in the position manager
/// </summary>
public class ApplicationHandler : NetworkBehaviour
{
    public float[] applicationPeriods = new float[] { };
    [Header("The application length, in periods")]
    public float appLength = 1.833334f;
    int periodsPassed = 0;
    PositionManager positionManager;
    ApplicationManager applicationManager;
    GameManager gameManager;
    float gameTimeAppLength;

    public override void Spawned()
    {
        positionManager = FindAnyObjectByType<PositionManager>();
        applicationManager = FindAnyObjectByType<ApplicationManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        gameManager.OnChangeDay += ResetPeriodsTracker;
        gameTimeAppLength = appLength * gameManager.hourLength;
        for (int i = 0; i < applicationPeriods.Length; i++)
        {
            applicationPeriods[i] *= gameManager.hourLength;
        }
    }

    public override void FixedUpdateNetwork()
    {
        CheckApplicationPeriod();
    }

    private void ResetPeriodsTracker()
    {
        periodsPassed = 0;
    }

    private void CheckApplicationPeriod()
    {
        if (periodsPassed >= applicationPeriods.Length) return;
        float localTime = gameManager.GetLocalTimeFromGameTime();
        if (applicationPeriods[periodsPassed] < localTime)
        {
            Debug.Log(applicationPeriods[periodsPassed] + " got passed by " + localTime);
            CreateApplications(applicationPeriods[periodsPassed]);
            periodsPassed++;
        }
    }

    // Adds to every application that can be applied to
    private void CreateApplications(float startTime)
    {
        startTime += gameManager.currentDay * 24f;
        int branchIndex = 0;
        foreach (Branch branch in positionManager.branches)
        {
            if (BranchAppliable(branch))
            {
                applicationManager.AddBranchApplication(branchIndex, startTime + gameTimeAppLength);
                Debug.Log("Application added for branch " + branch.name + " ends at period " + (startTime + gameTimeAppLength));
            }
            foreach (Job job in branch.jobs)
            {
                if (JobAppliable(job)) applicationManager.AddApplication(job.handler, startTime + gameTimeAppLength);
            }
            branchIndex++;
        }
    }

    private bool BranchAppliable(Branch branch)
    {
        if (branch.maxPlayers == -1) return true;
        // Returns true when the branch is not maxed out
        return branch.maxPlayers > branch.players.Count;
    }

    private bool JobAppliable(Job job)
    {
        return job.maxPlayers > job.assignedPlayers.Count;
    }
}
