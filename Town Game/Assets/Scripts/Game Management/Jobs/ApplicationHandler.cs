using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// Creates applications for the corresponding job handler
/// </summary>
[RequireComponent(typeof(JobHandler))]
public class ApplicationHandler : NetworkBehaviour
{
    Vector2Int jobReference;
    JobHandler jobHandler;
    PositionManager positionManager;
    ApplicationManager applicationManager;

    public override void Spawned()
    {
        jobHandler = GetComponent<JobHandler>();
        positionManager = FindAnyObjectByType<PositionManager>();
        applicationManager = FindAnyObjectByType<ApplicationManager>();

        jobReference = positionManager.GetJobHandlerFromRef(jobHandler);
    }

    public override void FixedUpdateNetwork()
    {
        
    }
}
