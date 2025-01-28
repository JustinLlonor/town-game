//using Photon.Pun;
//using Photon.Realtime;
using System.Collections.Generic;
using Fusion;

[System.Serializable]
public class ItemData
{
    public Dictionary<string, string> metadata = new Dictionary<string, string>();
    public List<PlayerRef> fingerprints = new List<PlayerRef>();
}
