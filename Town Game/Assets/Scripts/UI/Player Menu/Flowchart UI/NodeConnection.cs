using UnityEngine;

public struct NodeConnection
{
    public string connectedNodeName;
    public string connectionName;

    public NodeConnection(string connectedNodeName, string connectionName)
    {
        this.connectedNodeName = connectedNodeName;
        this.connectionName = connectionName;
    }
}
