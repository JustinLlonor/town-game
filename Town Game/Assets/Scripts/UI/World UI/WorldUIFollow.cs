using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUIFollow : MonoBehaviour
{
    public float yOffset;
    private Vector3 followTarget;
    public float speed = 10f;

    private void Update()
    {
        if (transform.position != followTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, followTarget, speed * Time.deltaTime);
        }
    }

    public void SetPosition(Vector3 newPos)
    {
        transform.position = new Vector3(newPos.x, newPos.y + yOffset, newPos.z);
    }

    public void SetTarget(Vector3 newTarget)
    {
        followTarget = newTarget + Vector3.up * yOffset;
    }
}
