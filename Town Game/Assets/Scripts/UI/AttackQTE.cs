using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackQTE : MonoBehaviour
{
    public RectTransform slider;
    public RectTransform sliderStart;
    public RectTransform sliderEnd;
    public RectTransform target;
    bool sliderMoving = false;
    float sliderSpeed;
    float sliderX;

    public void Init(float sliderSpeed, float targetLength)   
    {
        slider.position = sliderStart.position;
        sliderX = sliderStart.position.x;
        sliderMoving = true;
        this.sliderSpeed = sliderSpeed;
        float randomPosition = Random.Range(0f, ((RectTransform)target.parent).sizeDelta.x - targetLength); // Random position for the target from the domain
        target.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, randomPosition, targetLength);
    }

    private void Update()
    {
        if (sliderMoving)
        {
            sliderX += Time.deltaTime * sliderSpeed;
            slider.position = new Vector2(sliderX, slider.position.y);
            if (slider.position.x > sliderEnd.position.x)
            {
                StopSliderMoving();
            }
        }
    }

    void StopSliderMoving()
    {
        sliderMoving = false; // Add animations later
        gameObject.SetActive(false);
    }

    public bool GetSliderSuccess()
    {
        if (!sliderMoving) return false;
        StopSliderMoving(); // Stop the slider always when there is an attempt
        if (PositionWithinTarget(slider.position))
        {
            return true; // If slider is within target, hit is successful
        }

        return false;
    }

    private bool PositionWithinTarget(Vector2 position)
    {
        return RectTransformUtility.RectangleContainsScreenPoint(target, position);
    }
}
