//using Photon.Pun;
//using Photon.Realtime;
using System.Collections.Generic;
using Fusion;

[System.Serializable]
public struct ItemData
{
    public NetworkDictionary<string, string> metadata;
    public List<PlayerRef> fingerprints;

    public ItemData(NetworkDictionary<string, string> metadata, List<PlayerRef> fingerprints)
    {
        this.metadata = metadata;
        this.fingerprints = fingerprints;
    }
}
