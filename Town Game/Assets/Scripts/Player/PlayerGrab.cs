using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerGrab : NetworkBehaviour
{
    public float maxGrabDistance = 2f;
    public float grabTerminationDistance = 3f;
    [Networked] public bool isGrabbing { get; set; } = false;
    [Networked] public NetworkId grabbedObject { get; set; }
    [Networked] public NetworkBehaviourId grabbedBehaviour { get; set; }
    [Networked] public float grabbedDistance { get; set; }
    public Player player;
    public InteractableFinder inf;
    public LayerMask environmentLayer;

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;
        // find the grabbable
        NetworkBehaviour foundBehaviour;
        if (!Runner.TryFindBehaviour(grabbedBehaviour, out foundBehaviour)) return;
        // Grabber override checks first, then distance checks, then update grab
        Grabbable grabbable = (Grabbable)foundBehaviour;
        CheckGrabber(grabbable);
        CheckDistance(grabbable);
        UpdateGrab(grabbable);
    }

    private void CheckDistance(Grabbable grabbable)
    {
        if (!isGrabbing) return;
        float distance = Vector3.Distance(inf.trackedTransform.position, grabbable.transform.position);
        if (distance > grabTerminationDistance)
        {
            ReleaseGrabWithObject(grabbable);
        }
    }

    /// <summary>
    /// Checks the grabbable if it has been grabbed by another player
    /// </summary>
    private void CheckGrabber(Grabbable grabbable)
    {
        if (!isGrabbing) return;
        if (grabbable.grabber.Equals(player.owner)) return;
        ReleaseGrab(); // release grab without changing
    }

    /// <summary>
    /// Sets the grab point on the grabbable
    /// </summary>
    private void UpdateGrab(Grabbable grabbable)
    {
        if (!isGrabbing) return;
        Transform rayTransform = inf.trackedTransform;
        Vector3 rayDirection = inf.forwardDirection;
        Vector3 grabPoint = rayTransform.position + rayDirection * grabbedDistance;
        grabbable.grabPoint = grabPoint;
        Debug.DrawLine(grabPoint, grabPoint + Vector3.up);
    }

    /// <summary>
    /// Casts a ray to check for grabbable objects
    /// </summary>
    /// <returns>True if we grabbed onto something</returns>
    public bool CheckGrab()
    {
        if (isGrabbing) return false;
        Transform castTransform = inf.trackedTransform;
        Vector3 castDirection = inf.forwardDirection;
        RaycastHit hit;
        if (!Physics.Raycast(castTransform.position, castDirection, out hit, maxGrabDistance, environmentLayer)) return false;
        if ((hit.collider.gameObject.tag != "Grabbable")) return false;
        Grabbable grabbable = hit.collider.GetComponent<Grabbable>();
        if (!grabbable.canGrab) return false;
        grabbedBehaviour = grabbable.Id;
        grabbable.grabber = player.owner;
        grabbedObject = hit.collider.GetComponent<NetworkObject>().Id;
        isGrabbing = true;
        grabbedDistance = Vector3.Distance(castTransform.position, hit.point);
        return true;
    }

    public void CheckRelease()
    {
        if (isGrabbing)
        {
            ReleaseGrabWithObject();
        }
    }

    /// <summary>
    /// Releases the grab and sets the grabber variable on the grab object to none.
    /// </summary>
    /// <param name="grabbable"></param>
    public void ReleaseGrabWithObject(Grabbable grabbable = null)
    {
        if (!isGrabbing) return;
        if (grabbable == null)
        {
            NetworkBehaviour foundBehaviour;
            Runner.TryFindBehaviour(grabbedBehaviour, out foundBehaviour);
            grabbable = (Grabbable)foundBehaviour;
        }
        grabbable.grabber = PlayerRef.None;
        ReleaseGrab();
    }

    /// <summary>
    /// Releases the grab. 
    /// Should be called when the player gets out of range of the object or when the player stops holding grab,
    /// or when another player starts a grab
    /// </summary>
    public void ReleaseGrab()
    {
        isGrabbing = false;
    }
}
