using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

[System.Serializable]
public class ItemData
{
    public Dictionary<string, string> metadata = new Dictionary<string, string>();
    public List<Player> fingerprints = new List<Player>();
}
