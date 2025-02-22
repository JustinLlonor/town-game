using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class SIVisuals : MonoBehaviour
{
    public ItemObservable itemObservable;
    public int trackedSIIndex;
    public SIAnimationData[] progressAnimations;
    float previousProgress = 0f;

    [System.Serializable]
    public struct SIAnimationData
    {
        public float progressThreshold;
        public string animationState;
        public Animator animator;
    }

    private void Awake()
    {
        itemObservable.onSIUpdate += OnProgressUpdate;
    }

    private void OnProgressUpdate(int index, float progress)
    {
        if (index != trackedSIIndex) return;
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

    private void ResetAnimationState(Animator animator)
    {
        animator.Rebind();
        animator.Update(0f);
    }
}
