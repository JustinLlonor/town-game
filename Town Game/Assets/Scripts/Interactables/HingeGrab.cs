using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Unity.VisualScripting;

public class HingeGrab : GrabPoint
{
    public Rigidbody rb;
    [Networked] public bool isClosing { get; set; } = false;
    [Tooltip("The speed at which the door rotates in the direction of a grab")]
    public float grabVel = 5f;
    [Tooltip("The closeness to the close angle required to snap to close")]
    public float snapCloseness = 5f;
    public float snapSpeed = 5f;
    private float snapCloseDistance = 0.05f;
    [SerializeField] private float closeAngle = 0f;
    public Transform rotationGuide;
    public GameObject rotationGraphicsObject;
    public GameObject graphicsObject;

    private void Awake()
    {
        graphicsObject.SetActive(true);
        if (rotationGraphicsObject != null) rotationGraphicsObject.SetActive(false);
    }

    public override void Spawned()
    {
        rb.isKinematic = false;
        //JointLimits limits = GetComponent<HingeJoint>().limits;
        //Debug.Log(limits.min);
        //Debug.Log(limits.max);
        //Debug.Log(rotationGuide.localEulerAngles.y);
        //limits.min = limits.min + rotationGuide.localEulerAngles.y;
        //limits.max = limits.max + rotationGuide.localEulerAngles.y;
        //GetComponent<HingeJoint>().limits = limits;
        closeAngle = rotationGuide.eulerAngles.y;
        rb.isKinematic = true;
        ForceClose();
    }

    public override void FixedUpdateNetwork()
    {
        KinematicCheck();
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
        Debug.Log("Force closing");
        isClosing = false;
        isOpen = false;
        rb.rotation = Quaternion.Euler(new Vector3(0f, closeAngle, 0f));
        transform.rotation = Quaternion.Euler(new Vector3(0f, closeAngle, 0f));
    }

    private void CheckSnap()
    {
        if (!isOpen) return;
        // If the close angle is next to snap closenessz
        if (Mathf.Abs(Mathf.DeltaAngle(rb.rotation.eulerAngles.y, closeAngle)) < snapCloseness)
        {
            StartClosing();
        }
    }

    private void GrabBehaviour()
    {
        if (!grabbable.IsGrabbed()) return;
        Vector3 grabCoords = grabbable.grabPoint;
        Vector2 doorToGrab = new Vector2(rb.position.x - grabCoords.x, rb.position.z - grabCoords.z);
        float targetAngle = Mathf.Atan2(doorToGrab.y, doorToGrab.x) * -Mathf.Rad2Deg - 90f;
        //float newAngle = Mathf.MoveTowardsAngle(rb.rotation.eulerAngles.y, -targetAngle, grabSpeed * Runner.DeltaTime);
        float angleDelta = Mathf.DeltaAngle(rb.rotation.eulerAngles.y, targetAngle) * grabVel; // increase toward max, decrease toward min
        //newAngle = ClampAngle(newAngle, closeAngle, maxAngle, angleDelta);
        Debug.Log(angleDelta);
        rb.angularVelocity = new Vector3(0f, angleDelta, 0f);
    }

    private void KinematicCheck()
    {
        if (isOpen)
        {
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
        if (Mathf.Abs(Mathf.DeltaAngle(rb.rotation.eulerAngles.y, closeAngle)) < snapCloseDistance)
        {
            ForceClose();
        }
        else
        {
            rb.MoveRotation(Quaternion.Euler(new Vector3(0f, currentYRot, 0f)));
        }
    }

    private float ClampAngle(float current, float min, float max, float angleDelta)
    {
        // [0, 360)
        while (current >= 360f) current -= 360f;
        while (current < 0f) current += 360f;
        if (current < min || current >= max)
        {
            if (angleDelta > 0f)
            {
                return max;
            }
            return min;
        }
        return current;
    }
}
