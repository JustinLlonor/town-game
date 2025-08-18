using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;

public class ProgressHandler : NetworkBehaviour
{
    [Header("Settings")]
    [Tooltip("Events called on the server side when a certain threshold is reached")]
    public float[] eventThresholds;
    [Tooltip("The default rate of progress when a player clicks on the object. How much progress is cleared in a second, with the max progress being 100")]
    public float defaultRate = 10f;
    public float defaultRateSecondary = 0f;
    [Tooltip("The rate of progress when this handler is untouched")]
    public float untouchedRate = 0f;
    [Tooltip("Item attributes and their corresponding rate modifications")]
    public ItemAttributeRate[] attributeRates = new ItemAttributeRate[0];
    [Tooltip("Items and their corresponding rate modifications")]
    public ItemRate[] itemRates = new ItemRate[0];
    /// <summary>
    /// A number from 0-100 inclusive indicating the progress of this progress handler
    /// </summary>
    [Networked] public float progress { get; set; }
    /// <summary>
    /// If this is disabled, then this progress handler cannot be interacted with
    /// </summary>
    [Networked] public bool canProgress { get; set; } = true;
    private float previousProgress;
    /// <summary>
    /// Called when a threshold is reached exactly at the threshold
    /// </summary>
    public ThresholdEvent onThresholdReach;
    /// <summary>
    /// Called when a threshold is passed by adding past the threshold
    /// </summary>
    public ThresholdEvent onThresholdPassAdd;
    /// <summary>
    /// Called when a threshold is passed by subtracting past the threshold
    /// </summary>
    public ThresholdEvent onThresholdPassSubtract;
    /// <summary>
    /// When this is true, the progress handler has been touched on this frame
    /// </summary>
    private bool touchedOnFrame = false;
    ProgressManager progressManager;

    public delegate void ThresholdEvent(float value);

    [System.Serializable]
    public struct ItemAttributeRate
    {
        public ItemAttribute attribute;
        public bool primaryUse;
        public float modifiedRate;
    }

    [System.Serializable]
    public struct ItemRate
    {
        public Item item;
        public bool primaryUse;
        public float modifiedRate;
    }

    // For dictionary keys in case the struct lists get really big??? but honestly don't need these
    public struct ItemUse : IEquatable<ItemUse>
    {
        public Item item;
        public bool primaryUse;

        public ItemUse(Item item, bool primaryUse)
        {
            this.item = item;
            this.primaryUse = primaryUse;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemUse use && Equals(use);
        }

        public bool Equals(ItemUse other)
        {
            return EqualityComparer<Item>.Default.Equals(item, other.item) &&
                   primaryUse == other.primaryUse;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(item, primaryUse);
        }

        public static bool operator ==(ItemUse left, ItemUse right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemUse left, ItemUse right)
        {
            return !(left == right);
        }
    }

    public struct ItemAttributeUse : IEquatable<ItemAttributeUse>
    {
        public ItemAttribute atribute;
        public bool primaryUse;

        public ItemAttributeUse(ItemAttribute atribute, bool primaryUse)
        {
            this.atribute = atribute;
            this.primaryUse = primaryUse;
        }

        public override bool Equals(object obj)
        {
            return obj is ItemAttributeUse use && Equals(use);
        }

        public bool Equals(ItemAttributeUse other)
        {
            return atribute == other.atribute &&
                   primaryUse == other.primaryUse;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(atribute, primaryUse);
        }

        public static bool operator ==(ItemAttributeUse left, ItemAttributeUse right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ItemAttributeUse left, ItemAttributeUse right)
        {
            return !(left == right);
        }
    }

    public override void Spawned()
    {
        progressManager = FindAnyObjectByType<ProgressManager>();
        previousProgress = progress;
        if (!Runner.IsServer) return;
        progressManager.AddHandler(this);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Runner.IsServer) return;
        ProgressEvents();
        if (touchedOnFrame)
        {
            touchedOnFrame = false;
            return;
        }
        UntouchedRate();
    }

    /// <summary>
    /// Changes progress based on the item, use button, and delta time
    /// </summary>
    /// <param name="heldItem"></param>
    /// <param name="primaryUse"></param>
    /// <param name="deltaTime"></param>
    /// <returns>True if the progress can continue, false if it can't</returns>
    public bool ProcessProgress(Item heldItem, bool primaryUse, float deltaTime)
    {
        touchedOnFrame = true;
        float rate = GetRate(heldItem, primaryUse);
        progress += rate * deltaTime;
        progress = Mathf.Clamp(progress, 0f, 100f);
        return UseChanges(rate);
    }

    /// <summary>
    /// Returns true if the use with the following item and use button can change progress
    /// </summary>
    /// <param name="heldItem"></param>
    /// <param name="primaryUse"></param>
    /// <returns></returns>
    public bool UseChanges(Item heldItem, bool primaryUse)
    {
        float rate = GetRate(heldItem, primaryUse);
        return UseChanges(rate);
    }

    private bool UseChanges(float rate)
    {
        if ((rate > 0f) && (progress == 100f)) return false;
        if ((rate < 0f) && (progress == 0f)) return false;
        if (rate == 0f) return false;
        return true;
    }

    private float GetRate(Item heldItem, bool primaryUse)
    {
        // Empty hand
        if (heldItem == null)
        {
            if (primaryUse) return defaultRate;
            return defaultRateSecondary;
        }
        // Item rates
        foreach (ItemRate itemRate in itemRates)
        {
            if (heldItem != itemRate.item) continue;
            if (primaryUse != itemRate.primaryUse) continue;
            return itemRate.modifiedRate;
        }
        // Check item attributes
        foreach (ItemAttributeRate attributeRate in attributeRates)
        {
            if (heldItem.attributes == null) break; // if the list is empty
            if (!heldItem.attributes.Contains(attributeRate.attribute)) continue; // If the item doesn't contain the attribute
            if (primaryUse != attributeRate.primaryUse) continue;
            return attributeRate.modifiedRate;
        }
        if (primaryUse) return defaultRate;
        return defaultRateSecondary;
    }

    private void ProgressEvents()
    {
        if (previousProgress == progress) return;
        foreach (float threshold in eventThresholds)
        {
            // Add pass events
            if (progress > previousProgress)
            {
                if ((progress > threshold) && (previousProgress <= threshold))
                {
                    onThresholdPassAdd?.Invoke(threshold);
                }
            }
            else if (progress < previousProgress) // Subtract pass events
            {
                if ((progress < threshold) && (previousProgress >= threshold))
                {
                    onThresholdPassSubtract?.Invoke(threshold);
                }
            }
            if (progress == threshold) // Reach event
            {
                onThresholdReach?.Invoke(threshold);
            }
        }
        previousProgress = progress;
    }

    private void UntouchedRate()
    {
        progress += untouchedRate * Runner.DeltaTime;
        progress = Mathf.Clamp(progress, 0f, 100f);
    }
}
