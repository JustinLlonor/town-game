//using Photon.Pun;
//using Photon.Realtime;
using System.Collections.Generic;
using Fusion;

[System.Serializable]
public struct ItemData : INetworkStruct
{
    [Networked, Capacity(10)] public NetworkDictionary<NetworkString<_4>, int> metadata => default;
    [Networked, Capacity(20)] public NetworkLinkedList<PlayerRef> fingerprints => default;


    public ItemData(NetworkDictionary<NetworkString<_4>, int> metadata, NetworkLinkedList<PlayerRef> fingerprints)
    {
        foreach (KeyValuePair<NetworkString<_4>, int> pair in metadata)
        {
            metadata.Add(pair.Key, pair.Value);
        }
        foreach (PlayerRef player in fingerprints)
        {
            fingerprints.Add(player);
        }
    }

}
