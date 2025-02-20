using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            GetComponent<Observable>().StartObservation();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            GetComponent<Observable>().ExitObservation(true);
        }
    }
}
