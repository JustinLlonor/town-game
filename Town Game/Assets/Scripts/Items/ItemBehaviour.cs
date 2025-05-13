using UnityEngine;

public abstract class ItemBehaviour : ScriptableObject
{
    public abstract void Initialize(GameObject playerObject);
    public abstract void OnUse();
    public abstract void OnSecondaryUse();
    public abstract void Deinitialize();
}
