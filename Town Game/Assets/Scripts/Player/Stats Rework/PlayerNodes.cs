using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerNodes : NetworkBehaviour
{
    [Networked, Capacity(12)] public NetworkLinkedList<Node> nodes => default;
    public NodeInfo[] nodeInfo;
    public ConnectionInfo[] connectionInfo;
    public NodeThresholdEvent OnNodeCrossThreshold;

    private int idCounter = 0;
    GameManager gameManager;

    public delegate void NodeThresholdEvent(Node node, float thresholdCrossed);

    public override void Spawned()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        OnNodeCrossThreshold += CheckZeroDestroy;
    }

    public override void FixedUpdateNetwork()
    {
        UpdateNodes();
    }

    private void UpdateNodes()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            Node currentNode = nodes[i];
            // Calculate baseline rate
            if (currentNode.baselineRate != 0f)
            {
                float previousValue = currentNode.value;
                float unitsPerSecond = currentNode.baselineRate / gameManager.hourLength;
                currentNode.AddValue(Runner.DeltaTime * unitsPerSecond);
                // Threshold events
                if (!Runner.IsResimulation)
                {
                    foreach (float tEvent in currentNode.thresholdEvents)
                    {
                        float minValue;
                        float maxValue;
                        if (previousValue > currentNode.value)
                        {
                            minValue = currentNode.value;
                            maxValue = previousValue;
                        }
                        else if (previousValue < currentNode.value)
                        {
                            minValue = previousValue;
                            maxValue = currentNode.value;
                        }
                        else
                        {
                            continue;
                        }
                        if (tEvent >= minValue && tEvent <= maxValue)
                        {
                            OnNodeCrossThreshold?.Invoke(currentNode, tEvent);
                        }
                    }
                }
            }
            float nodeProgress = currentNode.value / 100f;
            // Connections
            foreach (var kvp in currentNode.connections)
            {
                int affectedNodeIndex = GetNodeIndexFromId(kvp.Key);
                Node affectedNode = nodes[affectedNodeIndex];
                ConnectionInfo connectionInfo = GetConnectionInfo(kvp.Value);
                float evaluatedRate = connectionInfo.effectLevels.Evaluate(nodeProgress);
                float unitsPerSecond = evaluatedRate / gameManager.hourLength;
                affectedNode.AddValue(Runner.DeltaTime * unitsPerSecond);
                nodes.Set(affectedNodeIndex, affectedNode);
            }
            nodes.Set(i, currentNode);
        }
    }

    /// <summary>
    /// Checks the node 
    /// </summary>
    /// <param name="node"></param>
    /// <param name="thresholdCrossed"></param>
    private void CheckZeroDestroy(Node node, float thresholdCrossed)
    {
        if (thresholdCrossed != 0f) return;
        NodeInfo info = GetNodeInfo(node.infoIndex);
        if (!info.destroyOnZero) return;
        RemoveNode(node.id);
    }

    public int AddNode(string nameId)
    {
        NodeInfo info = GetNodeInfo(nameId);
        if (info == null)
        {
            Debug.LogError("Node nameId is invalid!");
            return -1;
        }
        Node newNode = new Node(idCounter++, Array.IndexOf(nodeInfo, info), info.startingValue, info.startingRate);
        if (info.destroyOnZero) newNode.thresholdEvents.Add(0f);
        nodes.Add(newNode);
        return -1;
    }

    public int AddNode(string nameId, List<float> thresholdEvents)
    {
        NodeInfo info = GetNodeInfo(nameId);
        if (info == null)
        {
            Debug.LogError("Node nameId is invalid!");
            return -1;
        }
        Node newNode = new Node(idCounter++, Array.IndexOf(nodeInfo, info), info.startingValue, info.startingRate);
        if (info.destroyOnZero && !thresholdEvents.Contains(0f)) newNode.thresholdEvents.Add(0f);
        foreach (float tEvent in thresholdEvents)
        {
            newNode.thresholdEvents.Add(tEvent);
        }
        nodes.Add(newNode);
        return -1;
    }

    public void RemoveNode(string nameId)
    {
        int nodeId = GetNodeId(nameId);
        if (nodeId == -1) return;
        RemoveNode(nodeId);
    }

    public void RemoveNode(int nodeId)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].id == nodeId)
            {
                nodes.Remove(nodes[i]);
                return;
            }
        }
    }

    public bool NodeExists(string nameId)
    {
        foreach (Node node in nodes)
        {
            NodeInfo info = GetNodeInfo(node.infoIndex);
            if (info.name == nameId) return true;
        }
        return false;
    }

    /// <summary>
    /// Connects two nodes together using their names
    /// </summary>
    /// <param name="fromNodeName"></param>
    /// <param name="toNodeName"></param>
    /// <param name="connectionName"></param>
    public void ConnectNode(string fromNodeName, string toNodeName, string connectionName)
    {
        int fromNodeId = GetNodeId(fromNodeName);
        int toNodeId = GetNodeId(toNodeName);
        if (fromNodeId == -1 || toNodeId == -1) return;
        ConnectNode(fromNodeId, toNodeId, connectionName);
    }

    /// <summary>
    /// Connects two nodes together using their ids
    /// </summary>
    /// <param name="fromNodeId"></param>
    /// <param name="toNodeId"></param>
    /// <param name="connectionName"></param>
    public void ConnectNode(int fromNodeId, int toNodeId, string connectionName)
    {
        int fromNodeIndex = GetNodeIndexFromId(fromNodeId);
        int toNodeIndex = GetNodeIndexFromId(toNodeId);
        if (fromNodeIndex == -1 || toNodeIndex == -1) return;
        int connectionIndex = GetConnectionIndexFromName(connectionName);
        if (connectionIndex == -1) return;
        Node fromNode = nodes[fromNodeIndex];
        fromNode.AddConnection(toNodeIndex, connectionIndex);
    }

    /// <summary>
    /// Gets the node id in the current flowchart with the corresponding name id
    /// </summary>
    /// <param name="nameId"></param>
    /// <returns></returns>
    public int GetNodeId(string nameId)
    {
        foreach (Node node in nodes)
        {
            NodeInfo info = GetNodeInfo(node.infoIndex);
            if (info.name == nameId) return node.id;
        }
        return -1;
    }

    public NodeInfo GetNodeInfo(string nameId)
    {
        foreach (NodeInfo info in nodeInfo)
        {
            if (info.name == nameId) return info;
        }
        return null;
    }

    public NodeInfo GetNodeInfo(int infoIndex)
    {
        return nodeInfo[infoIndex];
    }

    private ConnectionInfo GetConnectionInfo(string nameId)
    {
        foreach (ConnectionInfo info in connectionInfo)
        {
            if (info.name == nameId) return info;
        }
        return null;
    }

    private ConnectionInfo GetConnectionInfo(int connectionIndex)
    {
        return connectionInfo[connectionIndex];
    }

    private int GetNodeIndexFromId(int nodeId)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].id == nodeId)
            {
                return i;
            }
        }
        return -1;
    }

    private int GetConnectionIndexFromName(string connectionName)
    {
        for (int i = 0; i < connectionInfo.Length; i++)
        {
            if (connectionInfo[i].name == connectionName) return i;
        }
        return -1;
    }

    public Node GetNode(int id)
    {
        foreach (Node node in nodes)
        {
            if (node.id == id)
            {
                return node;
            }
        }
        return Node.None;
    }
}
