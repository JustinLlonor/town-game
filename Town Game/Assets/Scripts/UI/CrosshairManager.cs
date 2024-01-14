using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairManager : MonoBehaviour
{
    public GameObject defaultCrosshair;
    public GameObject interactCrosshair;
    public bool interactShowing = false;

    private void Update()
    {
        defaultCrosshair.SetActive(!interactShowing);
        interactCrosshair.SetActive(interactShowing);
    }

}
