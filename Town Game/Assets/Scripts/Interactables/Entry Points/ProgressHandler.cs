using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using System;
using System.Linq;
using WebSocketSharp;

public class ProgressHandler : NetworkBehaviour
{
    [Header("Settings")]
    [Tooltip("Events called on the server side when a certain threshold is reached")]
    public float[] eventThresholds = new float[] { 100f };
    public ProgressProfile progressProfile;
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
    /// Check functions to determine if a player can skip progress. 
    /// If at least one function is true, then the player will skip progress. Otherwise, the player will not skip progress.
    /// Make the function take a Player parameter, then return a bool.
    /// </summary>
    public PlayerCheck playerSkipChecks;
    /// <summary>
    /// When this is true, the progress handler has been touched on this frame
    /// </summary>
    private bool touchedOnFrame = false;
    ProgressManager progressManager;

    public delegate void ThresholdEvent(float value);
    public delegate bool PlayerCheck(Player player);


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

    public struct ProgressModifierInfo
    {
        public FilterInfo filterInfo;
        public string actionName;
        public int progressDelta;

        public ProgressModifierInfo(FilterInfo filterInfo, int progressDelta, string actionName = null)
        {
            this.filterInfo = filterInfo;
            this.progressDelta = progressDelta;
            this.actionName = actionName;
        }

        public static ProgressModifierInfo None
        {
            get
            {
                return new ProgressModifierInfo(FilterInfo.None, -99);
            }
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
            if (primaryUse) return progressProfile.defaultRate;
            return progressProfile.defaultRateSecondary;
        }
        // Item rates, prioritized over item attributes
        foreach (ItemRate itemRate in progressProfile.itemRates)
        {
            if (heldItem != itemRate.item) continue;
            if (primaryUse != itemRate.primaryUse) continue;
            return itemRate.modifiedRate;
        }
        // Check item attributes, attribute rates stack
        float modifiedSum = 0f;
        foreach (ItemAttributeRate attributeRate in progressProfile.attributeRates)
        {
            if (heldItem.attributes == null) break; // if the list is empty
            if (!heldItem.attributes.Contains(attributeRate.attribute)) continue; // If the item doesn't contain the attribute
            if (primaryUse != attributeRate.primaryUse) continue;
            modifiedSum += attributeRate.modifiedRate;
        }
        if (modifiedSum != 0f)
        {
            return modifiedSum;
        }
        if (primaryUse) return progressProfile.defaultRate;
        return progressProfile.defaultRateSecondary;
    }

    public ProgressModifierInfo GetModifierInfo(Item heldItem, bool primaryUse)
    {
        float foundRate = GetRate(heldItem, primaryUse);
        int modifierDelta = Mathf.Clamp(Mathf.RoundToInt((-foundRate) / 30f), -3, 3); // Negative, because displayed progress in ui is inverted
        ProgressModifierInfo output = new ProgressModifierInfo(FilterInfo.None, modifierDelta);
        // Set default action name. This will be overrided if an itemrate action name is set to something
        if (foundRate > 0f) output.actionName = progressProfile.progressAddAction;
        else output.actionName = progressProfile.progressSubtractAction;
        // returns the output with filter info set to none and action name to default
        if (heldItem == null)
        {
            return output;
        }
        // Item rates
        foreach (ItemRate itemRate in progressProfile.itemRates)
        {
            if (heldItem != itemRate.item) continue;
            if (primaryUse != itemRate.primaryUse) continue;
            output.filterInfo = new FilterInfo(heldItem);
            if (!itemRate.actionName.IsNullOrEmpty()) output.actionName = itemRate.actionName;
            return output;
        }
        // Check item attributes
        List<ItemAttribute> filteredAttributes = new List<ItemAttribute>();
        float modifiedSum = 0f;
        foreach (ItemAttributeRate attributeRate in progressProfile.attributeRates)
        {
            if (heldItem.attributes == null) break; // if the list is empty
            if (!heldItem.attributes.Contains(attributeRate.attribute)) continue; // If the item doesn't contain the attribute
            if (primaryUse != attributeRate.primaryUse) continue;
            filteredAttributes.Add(attributeRate.attribute);
            modifiedSum += attributeRate.modifiedRate;
        }
        if ((filteredAttributes.Count > 0) && (modifiedSum != 0f))
        {
            output.filterInfo = new FilterInfo(filteredAttributes);
        }
        return output;
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
        if (!canProgress) return;
        if (progress == 100f) return;
        progress += progressProfile.untouchedRate * Runner.DeltaTime;
        progress = Mathf.Clamp(progress, 0f, 100f);
    }

    /// <summary>
    /// Checks if progress can skip or not for this particular player.
    /// If at least one check delegate is true, then return true. Otherwise, return false.
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    public bool ProgressCanSkip(Player player)
    {
        if (playerSkipChecks != null)
        {
            // if at least one of the check delegates is true, then return true
            Delegate[] checkDelegates = playerSkipChecks.GetInvocationList();
            for (int i = 0; i < checkDelegates.Length; i++)
            {
                bool checkValid = (bool)checkDelegates[i].DynamicInvoke(player);
                if (checkValid) return true;
            }
        }
        return false;
    }
}
