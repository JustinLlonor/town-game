using System.Collections.Generic;
using System;

public struct SceneElementInfo : IEquatable<SceneElementInfo>
{
    public SceneElement element;
    public int sceneIndex;

    public SceneElementInfo(SceneElement element, int sceneIndex)
    {
        this.element = element;
        this.sceneIndex = sceneIndex;
    }

    public override bool Equals(object obj)
    {
        return obj is SceneElementInfo info && Equals(info);
    }

    public bool Equals(SceneElementInfo other)
    {
        return EqualityComparer<SceneElement>.Default.Equals(element, other.element) &&
               sceneIndex == other.sceneIndex;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(element, sceneIndex);
    }

    public static bool operator ==(SceneElementInfo left, SceneElementInfo right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SceneElementInfo left, SceneElementInfo right)
    {
        return !(left == right);
    }
}
