using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class HingeGrab : GrabPoint
{
    public Rigidbody rb;
    public HingeJoint hingeJoint;
    [Networked] public bool isClosing { get; set; } = false;

    [Tooltip("The angle at which the door is closed")]
    public float closeAngle = 0f;
    [Tooltip("The speed at which the door rotates in the direction of a grab")]
    public float grabSpeed = 500f;
    [Tooltip("The closeness to the close angle required to snap to close")]
    public float snapCloseness = 5f;
    public float snapSpeed = 5f;
    private float snapCloseDistance = 0.05f;
    private bool previousGrabbed = false;

    public override void FixedUpdateNetwork()
    {
        if (grabbable.IsGrabbed())
        {
            isOpen = true;
            isClosing = false;
        }
        else
        {
            CheckSnap();
        }
        CloseSnap();
        KinematicCheck();
        GrabBehaviour();
    }

    /// <summary>
    /// Starts the animation to close the door
    /// </summary>
    public void StartClosing()
    {
        if (!isOpen) return;
        isClosing = true;
    }

    public void ForceClose()
    {
        isClosing = false;
        isOpen = false;
        rb.rotation = Quaternion.Euler(new Vector3(0f, closeAngle, 0f));
        transform.rotation = Quaternion.Euler(new Vector3(0f, closeAngle, 0f));
    }

    private void CheckSnap()
    {
        if (!isOpen) return;
        // If the close angle is next to snap closeness
        if (Mathf.DeltaAngle(rb.rotation.eulerAngles.y, closeAngle) < snapCloseness)
        {
            StartClosing();
        }
    }

    private void GrabBehaviour()
    {
        if (!grabbable.IsGrabbed()) return;
        Vector3 grabCoords = grabbable.grabPoint;
        Vector2 doorToGrab = new Vector2(rb.position.x - grabCoords.x, rb.position.z - grabCoords.z);
        float targetAngle = Mathf.Atan2(doorToGrab.y, doorToGrab.x) * Mathf.Rad2Deg + 90f;
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation.eulerAngles.y, -targetAngle, grabSpeed * Runner.DeltaTime);
        newAngle = ClampAngle(newAngle, hingeJoint.limits.min, hingeJoint.limits.max);
        rb.MoveRotation(Quaternion.Euler(new Vector3(0f, newAngle, 0f)));
    }

    private void KinematicCheck()
    {
        if (isOpen)
        {
            if (grabbable.IsGrabbed())
            {
                //rb.angularVelocity = Vector3.zero;
                rb.isKinematic = true;
                return;
            }
            rb.isKinematic = false;
            return;
        }
        if ((isClosing) || (!isOpen))
        {
            rb.isKinematic = true;
            //rb.angularVelocity = Vector3.zero;
        }
    }

    private void CloseSnap()
    {
        if (!isOpen) return;
        if (!isClosing) return;
        float currentYRot = rb.rotation.eulerAngles.y;
        currentYRot = Mathf.MoveTowardsAngle(currentYRot, closeAngle, snapSpeed * Runner.DeltaTime);
        if (Mathf.DeltaAngle(rb.rotation.y, closeAngle) < snapCloseDistance)
        {
            ForceClose();
        }
        else
        {
            rb.MoveRotation(Quaternion.Euler(new Vector3(0f, currentYRot, 0f)));
        }
    }

    private float ClampAngle(float current, float min, float max)
    {
        float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        float hdtAngle = dtAngle * 0.5f;
        float midAngle = min + hdtAngle;

        float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);
        return current;
    }
}
