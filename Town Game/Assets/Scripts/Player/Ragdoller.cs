using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Ragdoller : MonoBehaviourPunCallbacks  
{
    public Transform currentRig; // The rig on the ragdoll
    private Transform targetRig;
    public List<Transform> targetBones;
    public List<Transform> currentBones;

    /// <summary>
    /// Sets the corpse's bones positions to the target rig's bone positions
    /// </summary>
    [PunRPC]
    public void SetPositionsToTarget(Transform tRig)
    {
        targetRig = tRig;
        if (targetRig == null)
        {
            Debug.LogError("Target rig not assigned!");
            return;
        }
        currentRig.position = targetRig.position;
        RagdollSetup();
        foreach (Transform t in targetBones)
        {
            string tName = t.name;
            bool foundName = false;
            foreach (Transform c in currentBones)
            {
                if (c.name == tName)
                {
                    foundName = true;
                    break;
                }
            }
            if (!foundName)
            {
                Debug.LogError("Inconsistent: " + tName);
                Debug.Log(t.gameObject.tag);
                return;
            }
        }
        for (int currentIndex = 0; currentIndex < targetBones.Count; currentIndex++)
        {
            currentBones[currentIndex].position = targetBones[currentIndex].position;
            currentBones[currentIndex].rotation = targetBones[currentIndex].rotation;
        }
    }

    public void RagdollSetup()
    {
        targetBones.Clear();
        AddTransform(targetRig, ref targetBones);
        AddTransform(currentRig, ref currentBones);
    }

    private void AddTransform(Transform target, ref List<Transform> transforms)
    {
        foreach (Transform child in target)
        {
            if (child.gameObject.tag != "Rig Ignore") transforms.Add(child);
            if (child.childCount > 0)
            {
                AddTransform(child, ref transforms);
            }
        }
    }
}
