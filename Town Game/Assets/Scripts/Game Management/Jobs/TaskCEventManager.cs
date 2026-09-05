using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class TaskCEventManager : NetworkBehaviour
{
    public delegate void TaskEvent(string taskId);
    public delegate void CompletionEvent(CompletionInfo completionInfo);

    public BranchManager branchManager;
    public TaskEvent onAssignTask;
    public TaskEvent onUnassignTask;
    public CompletionEvent onCompleteTask;

    public override void Spawned()
    {
        // Iterate over every branch and add the events
        foreach (JobBranch branch in branchManager.branches)
        {
            branch.branchHandler.onAssignTask += AssignTask;
            branch.branchHandler.onUnassignTask += UnassignTask;
            branch.branchHandler.onCompleteTask += CompleteTask;
        }
    }

    private void AssignTask(PlayerRef player, string task)
    {
        RPC_AssignTask(player, task);
    }

    private void UnassignTask(PlayerRef player, string task)
    {
        RPC_UnassignTask(player, task);
    }

    private void CompleteTask(PlayerRef player, CompletionInfo info)
    {
        RPC_CompleteTask(player, info);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_AssignTask([RpcTarget] PlayerRef player, string task)
    {
        onAssignTask?.Invoke(task);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_UnassignTask([RpcTarget] PlayerRef player, string task)
    {
        onUnassignTask?.Invoke(task);
    }

    [Rpc(sources: RpcSources.StateAuthority, targets: RpcTargets.All)]
    public void RPC_CompleteTask([RpcTarget] PlayerRef player, CompletionInfo info)
    {
        onCompleteTask?.Invoke(info);
    }
}
