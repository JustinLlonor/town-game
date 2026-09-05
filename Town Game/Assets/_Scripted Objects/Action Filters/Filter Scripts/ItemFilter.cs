using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemFilter : ScriptableObject
{
    public abstract bool ItemIsValid(Item item, ItemData data);

    public abstract bool ItemIsValid(Item item, ItemData data, out FilterInfo filterCause);
}
