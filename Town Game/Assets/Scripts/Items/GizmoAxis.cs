[System.Serializable]
public enum GizmoAxis
{
    PosX = 0,
    PosY = 1,
    PosZ = 2,
    NegX = 3,
    NegY = 4,
    NegZ = 5,
}

public static class GizmoAxisMethods
{
    public static bool IsNegative(this GizmoAxis axis)
    {
        return (int)axis > 2;
    }
}