using Fusion;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct JobApplication : INetworkStruct, IEquatable<JobApplication>
{
    /// <summary>
    /// The time, in game time, when the application will close
    /// </summary>
    public float deadline;
    /// <summary>
    /// The branch index
    /// </summary>
    public int branchReference;
    /// <summary>
    /// The job index within the branch. If this is -1, then this is an application for a branch
    /// </summary>
    public int jobReference;

    public JobApplication(float deadline, int branchReference, int jobReference)
    {
        this.deadline = deadline;
        this.branchReference = branchReference;
        this.jobReference = jobReference;
    }

    public override bool Equals(object obj)
    {
        return obj is JobApplication application && Equals(application);
    }

    public bool Equals(JobApplication other)
    {
        return deadline == other.deadline &&
               branchReference == other.branchReference &&
               jobReference == other.jobReference;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(deadline, branchReference, jobReference);
    }

    public static bool operator ==(JobApplication left, JobApplication right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(JobApplication left, JobApplication right)
    {
        return !(left == right);
    }

    public static JobApplication None
    {
        get
        {
            JobApplication result = new JobApplication(-1f, -1, -1);
            return result;
        }
    }
}
