using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class SIVisuals : MonoBehaviour
{
    public ItemObservable itemObservable;
    public int trackedSIIndex;
    public SIAnimationData[] progressAnimations;
    public SIAnimationDataContinuous[] continuousAnimations;
    float previousProgress = 0f;

    [System.Serializable]
    public struct SIAnimationData
    {
        public float progressThreshold;
        public string animationState;
        public Animator animator;
    }

    [System.Serializable]
    public struct SIAnimationDataContinuous
    {
        public string animationState;
        public string syncedParameter;
        [Tooltip("The progress point where the animation starts")]
        [Range(0f, 1f)]
        public float animationStartTime;
        [Range(0f, 1f)]
        [Tooltip("The progress point where the animation ends")]
        public float animationEndTime;
        public Animator animator;
    }

    private void Awake()
    {
        itemObservable.onSIUpdate += OnProgressUpdate;
    }

    private void OnProgressUpdate(int index, float progress)
    {
        if (index != trackedSIIndex) return;
        CheckDiscreteAnimations(progress);
        UpdateContinuousAnimations(progress);
    }

    private void CheckDiscreteAnimations(float progress)
    {
        foreach (SIAnimationData data in progressAnimations)
        {
            // Check if we passed the progress threshold, if so then play the animation
            if (previousProgress < progress)
            {
                if (data.progressThreshold > previousProgress && data.progressThreshold <= progress)
                {
                    ResetAnimationState(data.animator);
                    data.animator.Play(data.animationState);
                }
            }
            else
            { // PreviousProgress is above Progress
                if (data.progressThreshold < previousProgress && data.progressThreshold >= progress)
                {
                    ResetAnimationState(data.animator);
                    data.animator.Play(data.animationState);
                }
            }
        }
        previousProgress = progress;
    }

    private void UpdateContinuousAnimations(float progress)
    {
        foreach (SIAnimationDataContinuous data in continuousAnimations)
        {
            float parameterProgress;
            if (progress >= data.animationEndTime) parameterProgress = 0.999999f; // Set to this number, because for some reason setting to 1 makes it reset to the start of the animation
            else if (progress <= data.animationStartTime) parameterProgress = 0f;
            else
            {
                parameterProgress = (progress-data.animationStartTime)/(data.animationEndTime - data.animationStartTime);
            }
            data.animator.Play(data.animationState);
            data.animator.SetFloat(data.syncedParameter, parameterProgress);
        }
    }

    private void ResetAnimationState(Animator animator)
    {
        animator.Rebind();
        animator.Update(0f);
    }
}
