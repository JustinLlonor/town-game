using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColonFlash : MonoBehaviour
{
    public float flashFrequency = 1f;
    public Image img;
    float timer;

    private void Awake()
    {
        timer = flashFrequency;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            timer = flashFrequency;
            img.enabled = !img.enabled;
        }
    }
}
