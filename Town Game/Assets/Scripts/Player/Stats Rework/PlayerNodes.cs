using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerNodes : NetworkBehaviour
{
    [Networked, Capacity(15)] public NetworkLinkedList<Node> nodes => default;
    private List<int> previousNodes = new List<int>();
    public NodeInfo[] nodeInfo;
    public ConnectionInfo[] connectionInfo;
    public NodeThresholdEvent OnNodeCrossThreshold;
    // Client-sided events
    public NodesEvent onNodeAdd;
    public NodeEvent onNodeValueChange;
    public NodesEvent onNodeRemove;
    // Node ids and their values on the previous frame
    private Dictionary<int, float> previousValues = new Dictionary<int, float>();

    private int idCounter = 0;
    GameManager gameManager;

    public delegate void NodeEvent(Node node);
    public delegate void NodesEvent(List<int> nodeIds);
    public delegate void NodeThresholdEvent(Node node, float thresholdCrossed);

    public override void Spawned()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        OnNodeCrossThreshold += CheckZeroDestroy;
        AddNode("Health");
        AddNode("Hunger", new List<NodeConnection> { new NodeConnection("Health", "Nutrition") });
        AddNode("Thirst", new List<NodeConnection> { new NodeConnection("Health", "Nutrition") });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha7))
            AddNode("Test", new List<NodeConnection> { new NodeConnection("Hunger", "Nutrition") });
    }

    public override void FixedUpdateNetwork()
    {
        UpdateNodes();
        if (!HasInputAuthority) return;
        CheckNodeChanges();
        CheckPreviousValues();
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

    // Client sidede event
    private void CheckNodeChanges()
    {
        // Create int list of nodes
        List<int> newNodes = new List<int>();
        for (int i = 0; i < nodes.Count; i++)
        {
            newNodes.Add(nodes[i].id);
        }
        // Check if new nodes has previous nodes
        List<int> nodeRemoveEvent = new List<int>(); // Record every change in chunks so the calculation doesn't happen multiple times
        foreach (int pNode in previousNodes)
        {
            if (!newNodes.Contains(pNode)) nodeRemoveEvent.Add(pNode);
        }
        if (nodeRemoveEvent.Count > 0) onNodeRemove?.Invoke(nodeRemoveEvent);
        // Check if new node is in previous node
        List<int> nodeAddEvent = new List<int>();
        foreach (int nNode in newNodes)
        {
            if (!previousNodes.Contains(nNode)) nodeAddEvent.Add(nNode);
        }
        if (nodeAddEvent.Count > 0) onNodeAdd?.Invoke(nodeAddEvent);
        previousNodes = newNodes;
    }

    private void CheckPreviousValues()
    {
        Dictionary<int, float> modifications = new Dictionary<int, float>();
        foreach (var kvp in previousValues)
        {
            Node node = GetNode(kvp.Key);
            if (node.value != kvp.Value)
            {
                onNodeValueChange?.Invoke(node);
                modifications.Add(kvp.Key, node.value);
            }
        }
        foreach (var kvp in modifications)
        {
            previousValues[kvp.Key] = kvp.Value;
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
        AddPreviousValue(newNode);
        return -1;
    }

    public int AddNode(string nameId, List<NodeConnection> connections)
    {
        NodeInfo info = GetNodeInfo(nameId);
        if (info == null)
        {
            Debug.LogError("Node nameId is invalid!");
            return -1;
        }
        Node newNode = new Node(idCounter++, Array.IndexOf(nodeInfo, info), info.startingValue, info.startingRate);
        foreach (NodeConnection connection in connections)
        {
            newNode.AddConnection(GetNodeId(connection.connectedNodeName), GetConnectionIndex(connection.connectionName));
        }
        if (info.destroyOnZero) newNode.thresholdEvents.Add(0f);
        nodes.Add(newNode);
        AddPreviousValue(newNode);
        return -1;
    }

    public int AddNode(string nameId, List<NodeConnection> connections, List<float> thresholdEvents)
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
        AddPreviousValue(newNode);
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
                break;
            }
        }
        RemoveConnectionFromAll(nodeId);
        RemovePreviousValue(nodeId);
    }

    /// <summary>
    /// Removes the specified connection from all nodes
    /// </summary>
    /// <param name="nodeId"></param>
    private void RemoveConnectionFromAll(int nodeId)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            Node newNode = nodes[i];
            if (newNode.connections.ContainsKey(nodeId))
            {
                newNode.RemoveConnection(nodeId);
                nodes.Set(i, newNode);
            }
        }
    }

    private void AddPreviousValue(Node node)
    {
        previousValues.Add(node.id, node.value);
    }

    private void RemovePreviousValue(int id)
    {
        if (!previousValues.ContainsKey(id)) return;
        previousValues.Remove(id);
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
        int connectionIndex = GetConnectionIndex(connectionName);
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

    public ConnectionInfo GetConnectionInfo(string nameId)
    {
        foreach (ConnectionInfo info in connectionInfo)
        {
            if (info.name == nameId) return info;
        }
        return null;
    }

    public ConnectionInfo GetConnectionInfo(int connectionIndex)
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

    private int GetNodeIndexFromName(string nameId)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (GetNodeInfo(nodes[i].infoIndex).name == nameId)
            {
                return i;
            }
        }
        return -1;
    }

    private int GetConnectionIndex(string connectionName)
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

    public Node GetNode(string nameId)
    {
        foreach (Node node in nodes)
        {
            NodeInfo info = GetNodeInfo(node.infoIndex);
            if (info.name == nameId) return node;
        }
        return Node.None;
    }

    public void ChangeNodeValue(string nameId, float change)
    {
        int nodeIndex = GetNodeIndexFromName(nameId);
        Node newNode = nodes[nodeIndex];
        newNode.value += change;
        nodes.Set(nodeIndex, newNode);
    }
}
