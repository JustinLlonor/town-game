using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

/// <summary>
/// The networked structure of a placed node
/// </summary>
[System.Serializable]
public struct Node : INetworkStruct
{
    /// <summary>
    /// The ID of this node within the flowchart, to be accessed by other nodes
    /// </summary>
    public int id;
    /// <summary>
    /// The key is the id of the node this node is connecting to, the value is the index of connection
    /// </summary>
    [Networked, Capacity(5)] public NetworkDictionary<int, int> connections => default;
    /// <summary>
    /// The index from PlayerNodes to gather the information about this node
    /// </summary>
    public int infoIndex;
    /// <summary>
    /// The % of units this node has
    /// </summary>
    public float value;
    /// <summary>
    /// The rate that this node is changing every tick
    /// </summary>
    public float baselineRate;
    /// <summary>
    /// The list of all threshold events
    /// </summary>
    [Networked, Capacity(5)] public NetworkLinkedList<float> thresholdEvents => default;

    public void AddConnection(int connectedNode, int connectionIndex)
    {
        if (connections.ContainsKey(connectedNode)) return;
        connections.Add(connectedNode, connectionIndex);
    }

    public void RemoveConnection(int connectedNode)
    {
        if (!connections.ContainsKey(connectedNode)) return;
        connections.Remove(connectedNode);
    }

    /// <summary>
    /// Adds/subtracts value to this node with a clamp
    /// </summary>
    /// <param name="addedValue"></param>
    public void AddValue(float addedValue)
    {
        value = Mathf.Clamp(value + addedValue, 0, 100f);
    }

    public Node(int id, int infoIndex, float value, float baselineRate)
    {
        this.id = id;
        this.infoIndex = infoIndex;
        this.value = value;
        this.baselineRate = baselineRate;
    }

    public static Node None { 
        get
        {
            return new Node(-1, -1, -1f, 0f);
        }
    
    }
}
