using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public struct ProgressModifierInfo : IEquatable<ProgressModifierInfo>
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

    public override bool Equals(object obj)
    {
        return obj is ProgressModifierInfo info && Equals(info);
    }

    public bool Equals(ProgressModifierInfo other)
    {
        return filterInfo.Equals(other.filterInfo) &&
               ((actionName.IsNullOrEmpty() && other.actionName.IsNullOrEmpty()) || actionName == other.actionName) &&
               progressDelta == other.progressDelta;
    }

    public bool IsNone()
    {
        return progressDelta == -99;
    }

    public static bool operator ==(ProgressModifierInfo left, ProgressModifierInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ProgressModifierInfo left, ProgressModifierInfo right)
    {
        return !(left == right);
    }
}
