using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ApplicationManager : NetworkBehaviour
{
    [Networked, Capacity(30)]
    public NetworkLinkedList<JobApplication> applications => default;
    public ApplicationEvent onApplicationEnd;
    public Dictionary<JobApplication, List<PlayerRef>> applicants = new Dictionary<JobApplication, List<PlayerRef>>();
    PositionManager positionManager;
    GameManager gameManager;

    public delegate void ApplicationEvent(JobApplication application);

    private void Awake()
    {
        positionManager = FindAnyObjectByType<PositionManager>();
        gameManager = FindAnyObjectByType<GameManager>();
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;
        CheckApplications();
    }

    /// <summary>
    /// Adds the application to the application manager
    /// </summary>
    /// <param name="handler">The job handler for the job</param>
    /// <param name="duration"></param>
    public void AddApplication(JobHandler handler, float duration)
    {
        Vector2Int jobRef = positionManager.GetJobReference(handler);
        if (jobRef.Equals(new Vector2(-1, -1))) return;
        if (ApplicationExists(jobRef)) return;
        JobApplication newApp = new JobApplication(gameManager.gameTime + duration, jobRef.x, jobRef.y);
        applications.Add(newApp);
    }

    /// <summary>
    /// Submits an application from a job reference
    /// </summary>
    /// <param name="jobRef"></param>
    /// <param name="player"></param>
    public void SubmitApplication(Vector2Int jobRef, PlayerRef player)
    {
        JobApplication application = GetApplicationFromRef(jobRef);
        if (application.Equals(JobApplication.None)) return;
        SubmitApplication(application, player);
    }

    /// <summary>
    /// Submits an application from a job application object
    /// </summary>
    /// <param name="job"></param>
    /// <param name="player"></param>
    public void SubmitApplication(JobApplication job, PlayerRef player)
    {
        if (!applicants.ContainsKey(job))
        {
            applicants.Add(job, new List<PlayerRef>() { player });
            return;
        }
        // TODO: Check if player is qualified to submit an application for this job, check the branches, etc.
        applicants[job].Add(player);
    }

    /// <summary>
    /// Removes an applicant form a job application from the job reference
    /// </summary>
    /// <param name="jobRef"></param>
    /// <param name="player"></param>
    public void RemoveApplicant(Vector2Int jobRef, PlayerRef player)
    {
        JobApplication application = GetApplicationFromRef(jobRef);
        if (application.Equals(JobApplication.None)) return;
        RemoveApplicant(application, player);
    }

    /// <summary>
    /// Removes an application from a job application object
    /// </summary>
    /// <param name="job"></param>
    /// <param name="player"></param>
    public void RemoveApplicant(JobApplication job, PlayerRef player)
    {
        if (!applicants.ContainsKey(job))
        {
            return;
        }
        if (applicants[job].Contains(player)) applicants[job].Remove(player);
    }

    /// <summary>
    /// Removes the specified player from all applications
    /// </summary>
    /// <param name="player"></param>
    public void ClearApplicant(PlayerRef player)
    {
        foreach (JobApplication job in applications)
        {
            if (applicants[job].Contains(player)) applicants[job].Remove(player);
        }
    }

    public void AddBranchApplication(int index, float duration)
    {

    }

    /// <summary>
    /// Selects and hires a player who applied to the specified job application
    /// </summary>
    /// <param name="application"></param>
    public void HireAppliedPlayers(JobApplication application)
    {
        // TODO: Make this prioritize players who have less jobs/employment
    }

    /// <summary>
    /// Removes applications and hires them when they reach the deadline
    /// </summary>
    private void CheckApplications()
    {
        foreach (JobApplication application in applications)
        {
            if (gameManager.gameTime > application.deadline)
            {
                HireAppliedPlayers(application);
                // invokes the event
                onApplicationEnd?.Invoke(application);
                // removes from applications
                applications.Remove(application);
                // removes from applicant listings
                if (applicants.ContainsKey(application)) applicants.Remove(application);
            }
        }
    }

    /// <summary>
    /// Checks if an application for the specified job reference is already in progress
    /// </summary>
    /// <param name="jobRef"></param>
    /// <returns></returns>
    private bool ApplicationExists(Vector2Int jobRef)
    {
        foreach (JobApplication application in applications)
        {
            if (application.branchReference == jobRef.x && application.jobReference == jobRef.y)
            {
                Debug.LogWarning("You are trying to create an application for a job that already has an application!");
                return true;
            }
        }
        return false;
    }

    private JobApplication GetApplicationFromRef(Vector2Int jobRef)
    {
        foreach (JobApplication application in applications)
        {
            if (application.branchReference == jobRef.x && application.jobReference == jobRef.y)
            {
                return application;
            }
        }
        return JobApplication.None;
    }
}
