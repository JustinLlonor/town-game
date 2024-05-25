using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    public Light lightSource;
    Transform sunTransform;
    public Light nightLight;
    public Vector3 angleVector = Vector3.right;
    public float hideAngle;
    public float showAngle;
    public float maxBrightness;
    public float maxNightBrightness;
    public AnimationCurve brightnessCurve;
    public AnimationCurve nightCurve;
    public Gradient sunsetGradient;
    GameManager gm;

    private void Awake()
    {
        gm = FindObjectOfType<GameManager>();
        sunTransform = lightSource.transform;
    }

    private void Update()
    {
        CycleSun();
    }

    void CycleSun()
    {
        // Angle
        float newAngle = gm.GetDayProgress() * 360f - 90f;
        sunTransform.rotation = Quaternion.AngleAxis(newAngle, angleVector);
        if (newAngle > hideAngle)
        {
            lightSource.enabled = false;
            nightLight.intensity = maxNightBrightness;
            return;
        }
        if (newAngle > showAngle) lightSource.enabled = true;

        // Brightness
        float progress = sunTransform.eulerAngles.x / (hideAngle - showAngle);
        lightSource.intensity = maxBrightness * brightnessCurve.Evaluate(progress);
        lightSource.color = sunsetGradient.Evaluate(brightnessCurve.Evaluate(progress));
        nightLight.intensity = maxNightBrightness * nightCurve.Evaluate(progress);
    }
}
