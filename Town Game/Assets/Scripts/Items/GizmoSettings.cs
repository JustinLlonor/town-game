using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gizmo Settings", menuName = "Items/Gizmo Settings")]
public class GizmoSettings : ScriptableObject
{
    public Vector3 rotation;
    public CenterSettings centerSettings = new CenterSettings();
    public UpSettings upSettings = new UpSettings();
    public RotationSettings rotationSettings = new RotationSettings();

    [System.Serializable]
    public class CenterSettings
    {
        [Tooltip("When this is enabled, automatically centers the item relative to the rotation pivot on the x axis")]
        public bool centerX = true;
        [Tooltip("When this is enabled, automatically centers the item relative to the rotation pivot on the y axis")]
        public bool centerZ = true;
        public Vector2 displacement;
    }

    [System.Serializable]
    public class UpSettings
    {
        [Tooltip("The axis from the mesh data to use as reference to displace the item upwards")]
        public Axis upAxis;
        public float upDisplacement;
    }

    [System.Serializable]
    public class RotationSettings
    {
        public float initialRotation;
        public RotationLimit rotationLimit;
        public RotationLimit surfaceRotationLimit;
    }

    [System.Serializable]
    public enum Axis
    {
        X = 0,
        Y = 1,
        Z = 2,
        None = 4
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
}
