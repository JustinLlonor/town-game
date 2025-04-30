using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    private void Awake()
    {
        transform.GetChild(1).SetSiblingIndex(0);
    }
}
