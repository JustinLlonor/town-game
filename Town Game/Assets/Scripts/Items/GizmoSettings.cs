using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "New Gizmo Settings", menuName = "Items/Gizmo Settings")]
public class GizmoSettings : ScriptableObject
{
    public Vector3 rotation;
    public CenterSettings centerSettings = new CenterSettings();
    public UpSettings upSettings = new UpSettings();
    public RotationSettings rotationSettings = new RotationSettings();
    private AxisCalculation currentAxisCalculation = null;

    [System.Serializable]
    public class CenterSettings
    {
        [Tooltip("When this is enabled, automatically centers the item relative to the rotation pivot on the x axis")]
        public bool centerX = true;
        [Tooltip("When this is enabled, automatically centers the item relative to the rotation pivot on the y axis")]
        public bool centerZ = true;
        [Tooltip("The displacement of both the x and z axes when the centering is not enabled")]
        public Vector2 displacement;
    }

    [System.Serializable]
    public class UpSettings
    {
        public float upDisplacement;
        [Tooltip("When this is enabled, mesh data is used to calculate how far the gizmo must go up in order to be on the surface. " +
            "When this is disabled, the upDisplacement property will be used to determine the local Y.")]
        public bool useMeshData = true;
    }

    [System.Serializable]
    public class RotationSettings
    {
        public float initialRotation;
        public RotationLimit rotationLimit;
        public RotationLimit surfaceRotationLimit;
    }

    [System.Serializable]
    public struct RotationLimit
    {
        [Range(0.0f, 360.0f)]
        public float minRotation;
        [Range(0.0f, 360.0f)]
        public float maxRotation;
        [Tooltip("When inverted, the gap between the minRotation and maxRotation becomes the area where the rotation is not allowed.")]
        public bool inverted;

        public bool RotationWithinLimit(float rotation)
        {
            float rotationInt = Mathf.FloorToInt(rotation / 360f);
            float checkedRotation = rotation - (rotationInt * 360f);
            if (!inverted) return ((checkedRotation >= minRotation) && (checkedRotation <= maxRotation));
            return (checkedRotation <= minRotation) || (checkedRotation >= maxRotation);
        }
    }

    private class AxisCalculation
    {
        public GizmoAxis upAxis;
        public GizmoAxis xAxis;
        public GizmoAxis zAxis;
    }

    public GizmoAxis GetUpAxis()
    {
        AxisCalculation ac = GetAxisCalculation();
        return ac.upAxis;
    }

    /// <summary>
    /// Gets the axis from up settings to get the bounds we need to displace the collider on the x axis
    /// </summary>
    /// <returns></returns>
    public GizmoAxis GetXAxis()
    {
        AxisCalculation ac = GetAxisCalculation();
        return ac.xAxis;
    }

    /// <summary>
    /// Gets the axis from up settings to get the bounds we need to displace the collider on the z axis
    /// </summary>
    /// <returns></returns>
    public GizmoAxis GetZAxis()
    {
        AxisCalculation ac = GetAxisCalculation();
        return ac.zAxis;
    }

    private AxisCalculation GetAxisCalculation()
    {
        if (currentAxisCalculation != null) return currentAxisCalculation;
        AxisCalculation axisCalculation = new AxisCalculation();
        // up axis is posy, posx is x axis and pos z is z axis
        //                                                0                 1             2               3              4              5
        List<GizmoAxis> axes = new List<GizmoAxis>() { GizmoAxis.PosX, GizmoAxis.PosY, GizmoAxis.PosZ, GizmoAxis.NegX, GizmoAxis.NegY, GizmoAxis.NegZ };
        // pos y > pos z > neg y > neg z
        int xTurns = Mathf.RoundToInt(rotation.x / 90f);
        int[] turnedXIndices = new int[] { axes.IndexOf(GizmoAxis.PosY), axes.IndexOf(GizmoAxis.PosZ), 
            axes.IndexOf(GizmoAxis.NegY), axes.IndexOf(GizmoAxis.NegZ) };
        if (xTurns < 0)
        {
            Array.Reverse(turnedXIndices);
            xTurns *= -1;
        }
        for (int i = 0; i < xTurns; i++)
        {
            RotateAxis(axes, turnedXIndices);
        }
        // pos z > pos x > neg z > neg x
        int yTurns = Mathf.RoundToInt(rotation.y / 90f);
        // Uses indexOf because of the rotation system that Unity uses, now the gizmo axes will rotate the same with local rotation
        int[] turnedYIndices = new int[] { axes.IndexOf(GizmoAxis.PosZ), axes.IndexOf(GizmoAxis.PosX),
            axes.IndexOf(GizmoAxis.NegZ), axes.IndexOf(GizmoAxis.NegX) };
        if (yTurns < 0)
        {
            Array.Reverse(turnedYIndices);
            yTurns *= -1;
        }
        for (int i = 0; i < yTurns; i++)
        {
            RotateAxis(axes, turnedYIndices);
        }
        // pos x > pos y > neg x > neg y
        int zTurns = Mathf.RoundToInt(rotation.z / 90f);
        int[] turnedZIndices = new int[] { axes.IndexOf(GizmoAxis.PosX), axes.IndexOf(GizmoAxis.PosY),
            axes.IndexOf(GizmoAxis.NegX), axes.IndexOf(GizmoAxis.NegY) };
        if (zTurns < 0)
        {
            Array.Reverse(turnedZIndices);
            zTurns *= -1;
        }
        for (int i = 0; i < zTurns; i++)
        {
            RotateAxis(axes, turnedZIndices);
        }
        // Sets the axis calculation axes
        axisCalculation.upAxis = axes[1];
        axisCalculation.xAxis = axes[0];
        axisCalculation.zAxis = axes[2];
        currentAxisCalculation = axisCalculation;
        return axisCalculation;
    }

    private void RotateAxis(List<GizmoAxis> axes, int[] rotatedIndices)
    {
        GizmoAxis beginningAxis = axes[rotatedIndices[0]];
        axes[rotatedIndices[0]] = axes[rotatedIndices[3]];
        axes[rotatedIndices[3]] = axes[rotatedIndices[2]];
        axes[rotatedIndices[2]] = axes[rotatedIndices[1]];
        axes[rotatedIndices[1]] = beginningAxis;
    } 
}
