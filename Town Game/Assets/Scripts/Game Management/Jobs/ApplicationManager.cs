using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class ApplicationManager : NetworkBehaviour
{
    [Networked, Capacity(30)]
    public NetworkLinkedList<JobApplication> applications => default;
    public ApplicationEvent onApplicationEnd;
    public Dictionary<JobApplication, List<PlayerRef>> applicants = new Dictionary<JobApplication, List<PlayerRef>>();
    PositionManager positionManager;
    GameManager gameManager;
    PlayerManager playerManager;
    RunnerManager runnerManager;

    public delegate void ApplicationEvent(JobApplication application);

    public override void Spawned()
    {
        positionManager = FindAnyObjectByType<PositionManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        playerManager = FindAnyObjectByType<PlayerManager>();
        runnerManager = FindAnyObjectByType<RunnerManager>();
        runnerManager.onPlayerLeave += ClearApplicant;
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
        Vector2Int jobRef = positionManager.GetJobHandlerFromRef(handler);
        if (jobRef.Equals(new Vector2Int(-1, -1))) return;
        if (ApplicationExists(jobRef)) return;
        JobApplication newApp = new JobApplication(gameManager.gameTime + duration, jobRef.x, jobRef.y);
        applications.Add(newApp);
    }

    public void AddBranchApplication(int index, float duration)
    {
        if (index >= positionManager.branches.Length) return;
        Vector2Int appRef = new Vector2Int(index, -1);
        if (ApplicationExists(appRef)) return;
        JobApplication newApp = new JobApplication(gameManager.gameTime + duration, appRef.x, appRef.y);
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
        // If player is in different branch from the job, return
        int playerBranch = playerManager.playerProperties[player].branch;
        if (playerBranch != job.branchReference) return;
        if (!applicants.ContainsKey(job))
        {
            applicants.Add(job, new List<PlayerRef>() { player });
            return;
        }
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
        // Remove player from all applicants
        foreach (JobApplication job in applications)
        {
            if (applicants[job].Contains(player)) applicants[job].Remove(player);
        }
    }

    /// <summary>
    /// Selects and hires a player who applied to the specified job application
    /// </summary>
    /// <param name="application"></param>
    public void ProcessApplication(JobApplication application)
    {
        if (application.jobReference == -1)
        {
            ProcessBranchApplication(application);
            return;
        }
        List<PlayerRef> candidates = applicants[application];
        // Every index in this list is a list of player refs with that amount of employment.
        List<List<PlayerRef>> selectionList = new List<List<PlayerRef>>();
        foreach (PlayerRef player in candidates)
        {
            int jobCount = playerManager.playerProperties[player].jobs.Count;
            // repeat until selection list count is greater than job count by 1
            while (jobCount >= selectionList.Count)
            {
                selectionList.Add(new List<PlayerRef>());
            }
            selectionList[jobCount].Add(player);
        }

        // Select a specified number of players from selectionList, prioritizing those with lower employment first
        Job job = positionManager.GetJobFromRef(new Vector2Int(application.branchReference, application.jobReference));
        // The amount of players the job needs
        int numberSelected = job.maxPlayers - job.assignedPlayers.Count;
        if (numberSelected <= 0) return;
        List<PlayerRef> selectedPlayers = new List<PlayerRef>();
        int employmentLevel = 0;
        // Stops when the selected players count is equal to the number that we want to select, and when the job count index is equal to the selection list count
        while (numberSelected > selectedPlayers.Count && employmentLevel < selectionList.Count)
        {
            List<PlayerRef> employedPlayers = selectionList[employmentLevel];
            // If there are no more employed players to select at this level, then go to the next level
            if (employedPlayers.Count == 0)
            {
                employmentLevel++;
                continue;
            }
            // Select a random player from employedPlayers, then add it to the selected players list
            int selectedIndex = Random.Range(0, employedPlayers.Count);
            PlayerRef selectedPlayer = employedPlayers[selectedIndex];
            selectedPlayers.Add(selectedPlayer);
            // Lowest employed list index removed, number selected decremented
            selectionList[employmentLevel].RemoveAt(selectedIndex);
        }

        // Hire the selected players
        foreach (PlayerRef player in selectedPlayers) job.AddPlayer(player);
    }

    /// <summary>
    /// Selects randomly and indiscriminantly from every applicant to a branch to be enrolled into that branch.
    /// </summary>
    /// <param name="application"></param>
    public void ProcessBranchApplication(JobApplication application)
    {
        List<PlayerRef> candidates = new List<PlayerRef>(applicants[application]);
        PositionManager.Branch branch = positionManager.branches[application.branchReference];
        // # of players to select
        int numberSelected = branch.maxPlayers - branch.players.Count;
        List<PlayerRef> selectedPlayers = new List<PlayerRef>();
        while (numberSelected > selectedPlayers.Count && candidates.Count > 0)
        {
            int selectedIndex = Random.Range(0, candidates.Count);
            PlayerRef selectedPlayer = candidates[selectedIndex];
            selectedPlayers.Add(selectedPlayer);
            candidates.RemoveAt(selectedIndex);
        }

        foreach (PlayerRef player in selectedPlayers) positionManager.AddPlayerToBranch(player, branch);
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
                ProcessApplication(application);
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
