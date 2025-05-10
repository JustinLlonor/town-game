using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System.Linq;

public class ApplicationManager : NetworkBehaviour
{
    [Networked, Capacity(30)]
    public NetworkLinkedList<JobApplication> applications => default;
    [Networked, Capacity(30)]
    public NetworkDictionary<JobApplication, int> playerCounts => default;
    public ApplicationEvent onApplicationAdd;
    public ApplicationEvent onApplicationRemove;
    public Dictionary<JobApplication, List<PlayerRef>> applicants = new Dictionary<JobApplication, List<PlayerRef>>();
    PositionManager positionManager;
    GameManager gameManager;
    PlayerManager playerManager;
    RunnerManager runnerManager;
    List<JobApplication> previousApplications = new List<JobApplication>();
    bool init = false;

    public delegate void ApplicationEvent(JobApplication application);

    public override void Spawned()
    {
        positionManager = FindAnyObjectByType<PositionManager>();
        gameManager = FindAnyObjectByType<GameManager>();
        playerManager = FindAnyObjectByType<PlayerManager>();
        runnerManager = FindAnyObjectByType<RunnerManager>();
        runnerManager.onPlayerLeave += ClearApplicant;
        init = true;
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;
        CheckApplications();
    }

    private void FixedUpdate()
    {
        if (!init) return;
        CheckAppChanges();
    }

    /// <summary>
    /// Adds the application to the application manager
    /// </summary>
    /// <param name="handler">The job handler for the job</param>
    /// <param name="startTime"></param>
    public void AddApplication(JobHandler handler, float startTime)
    {
        Vector2Int jobRef = positionManager.GetJobHandlerFromRef(handler);
        if (jobRef.Equals(new Vector2Int(-1, -1))) return;
        if (ApplicationExists(jobRef)) return;
        JobApplication newApp = new JobApplication(startTime, jobRef.x, jobRef.y);
        applications.Add(newApp);
        playerCounts.Add(newApp, 1);
    }

    public void AddBranchApplication(int index, float startTime)
    {
        if (index >= positionManager.branches.Length) return;
        Vector2Int appRef = new Vector2Int(index, -1);
        if (ApplicationExists(appRef)) return;
        JobApplication newApp = new JobApplication(startTime, appRef.x, appRef.y);
        applications.Add(newApp);
        playerCounts.Add(newApp, 1);
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
        int playerBranch = positionManager.GetBranch(player);
        if ((playerBranch == job.branchReference) ^ (job.jobReference >= 0)) return;
        if (positionManager.PlayerHasJob(player, job.GetJobRef())) return;
        if (!applicants.ContainsKey(job))
        {
            Debug.Log("Added application for " + job.GetJobRef());
            applicants.Add(job, new List<PlayerRef>() { player });
            return;
        }
        applicants[job].Add(player);
        playerCounts.Set(job, playerCounts[job] + 1);
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
        if (applicants[job].Contains(player))
        {
            applicants[job].Remove(player);
            playerCounts.Set(job, playerCounts[job] - 1);
        }
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
            if (applicants[job].Contains(player))
            {
                applicants[job].Remove(player);
                playerCounts.Set(job, playerCounts[job] - 1);
            }
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
        List<PlayerRef> candidates = new List<PlayerRef>();
        if (applicants.ContainsKey(application)) candidates = applicants[application];
        // Every index in this list is a list of player refs with that amount of employment.
        List<List<PlayerRef>> selectionList = new List<List<PlayerRef>>();
        foreach (PlayerRef player in candidates)
        {
            int jobCount = positionManager.GetJobCount(player);
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

        RemoveApplication(application);
    }

    /// <summary>
    /// Selects randomly and indiscriminantly from every applicant to a branch to be enrolled into that branch.
    /// </summary>
    /// <param name="application"></param>
    public void ProcessBranchApplication(JobApplication application)
    {
        Debug.Log("processing branch app of index " + application.branchReference);
        List<PlayerRef> candidates = new List<PlayerRef>();
        if (applicants.ContainsKey(application))
        {
            Debug.Log("candidates list created");
            candidates = new List<PlayerRef>(applicants[application]);
        }
        Branch branch = positionManager.branches[application.branchReference];
        // # of players to select
        int numberSelected = branch.maxPlayers - branch.players.Count;
        if (branch.maxPlayers < 0) numberSelected = 20;
        Debug.Log("number selected: " + numberSelected);
        List<PlayerRef> selectedPlayers = new List<PlayerRef>();
        while (numberSelected > selectedPlayers.Count && candidates.Count > 0)
        {
            int selectedIndex = Random.Range(0, candidates.Count);
            PlayerRef selectedPlayer = candidates[selectedIndex];
            Debug.Log("adding selected player");
            selectedPlayers.Add(selectedPlayer);
            candidates.RemoveAt(selectedIndex);
        }

        foreach (PlayerRef player in selectedPlayers) positionManager.AddPlayerToBranch(player, branch);

        RemoveApplication(application);
    }

    private void RemoveApplication(JobApplication application)
    {
        if (applicants.ContainsKey(application))
        {
            applicants.Remove(application);
        }
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
                // removes from applications
                applications.Remove(application);
                playerCounts.Remove(application);
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

    private void CheckAppChanges()
    {
        // Adding check
        foreach (JobApplication app in applications)
        {
            if (!previousApplications.Contains(app))
            {
                previousApplications.Add(app);
                onApplicationAdd?.Invoke(app);
            }
        }
        // Removal check
        List<JobApplication> removalList = new List<JobApplication>();
        foreach (JobApplication app in previousApplications)
        {
            if (!applications.Contains(app))
            {
                removalList.Add(app);
                onApplicationRemove?.Invoke(app);
            }
        }
        foreach (JobApplication app in removalList)
        {
            previousApplications.Remove(app);
        }
    }
}
