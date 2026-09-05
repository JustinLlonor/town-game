using UnityEngine;
using Fusion;

public struct ItemInitInfo
{
    public GameObject player;
    public NetworkDictionary<NetworkString<_4>, int> metadata;
    public string item;

    public ItemInitInfo(GameObject player, NetworkDictionary<NetworkString<_4>, int> metadata,string item)
    {
        this.player = player;
        this.metadata = metadata;
        this.item = item;
    }
}
