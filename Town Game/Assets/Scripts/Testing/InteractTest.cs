using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractTest : MonoBehaviour
{
    public void Interaction()
    {
        FindObjectOfType<Rigidbody>().velocity = new Vector3(0f, 10f, 0f);
    }
}
