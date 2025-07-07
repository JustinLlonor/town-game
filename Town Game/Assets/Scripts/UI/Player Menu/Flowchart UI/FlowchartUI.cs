using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowchartUI : MonoBehaviour
{
    public string hpText = "Health";
    public Color increaseColor = Color.green;
    public Color decreaseColor = Color.red;
    public Vector2Int gridSize;
    public Vector2 cellSize;
    public Transform contentTransform;
    public Transform nodeHolder;
    public Transform arrowHolder;
    public GameObject nodePrefab;
    public GameObject arrowPrefab;
    public NodeGrid uiGrid;
    public Dictionary<int, PhysNode> uiObjects = new Dictionary<int, PhysNode>(); // node id and game object
    private Dictionary<int, List<ConnectionUI>> uiConnections = new Dictionary<int, List<ConnectionUI>>(); // node id and connection ui
    private Dictionary<int, string> previousStatus = new Dictionary<int, string>();
    PlayerNodes playerNodes;

    /// <summary>
    /// Called when ndoe info needs to be shown in the hotbar
    /// </summary>
    public NodeUIEvent onInfoSend;

    public delegate void NodeUIEvent(int id);

    public struct NodeGrid
    {
        public Vector2Int gridSize;
        public Dictionary<int, Vector2Int> placedNodes;
        private List<Vector2Int> occupiedLocations;
        private Vector2Int center;

        public NodeGrid(Vector2Int gridSize)
        {
            this.gridSize = gridSize;
            placedNodes = new Dictionary<int, Vector2Int>();
            occupiedLocations = new List<Vector2Int>();
            center = new Vector2Int(Mathf.FloorToInt(gridSize.x / 2f), Mathf.FloorToInt(gridSize.y / 2f));
        }

        public void PlaceNode(int nodeId, Vector2Int location)
        {
            if (placedNodes.ContainsKey(nodeId))
            {
                RemoveNode(nodeId);
            }
            occupiedLocations.Add(location);
            placedNodes.Add(nodeId, location);
        }

        public void RemoveNode(int nodeId)
        {
            if (!placedNodes.ContainsKey(nodeId)) return;
            if (occupiedLocations.Contains(placedNodes[nodeId]))occupiedLocations.Remove(placedNodes[nodeId]);
            placedNodes.Remove(nodeId);
        }

        public bool NodePlaced(int nodeId)
        {
            return placedNodes.ContainsKey(nodeId); // if contains key, node is placed
        }

        public Vector2Int GetCenter()
        {
            return center;
        }

        public bool TileOccupied(Vector2Int location)
        {
            return occupiedLocations.Contains(location); // if contains this location, occupied at that loc
        }

        /// <summary>
        /// Gets the symmetry value of a place location. Likely only works for odd numbered grid sizes
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        public int GetLocationSymmetry(Vector2Int location)
        {
            int output = 0;
            // check y axis reflection symmetry
            if (location.x != center.x)
            {
                int stepDistance = Mathf.FloorToInt(center.x - location.x);
                Vector2Int reflectedLocation = new Vector2Int(center.x + stepDistance, location.y);
                if (TileOccupied(reflectedLocation)) output++;
            }
            // check x axis reflection symmetry
            if (location.y != center.y)
            {
                int stepDistance = Mathf.FloorToInt(center.y - location.y);
                Vector2Int reflectedLocation = new Vector2Int(location.x, center.y + stepDistance);
                if (TileOccupied(reflectedLocation)) output++;
            }
            return output;
        }
    }

    // When the value for a node updates, see if its connections are updated
    private struct ConnectionUI
    {
        public int connectedNodeId;
        public int connectionIndex;
        public bool increaseIsGood;
        public PhysArrow arrow;
        public float previousConnectionDelta;

        public ConnectionUI(int connectedNodeId, int connectionIndex, bool increaseIsGood, PhysArrow arrow)
        {
            this.connectedNodeId = connectedNodeId;
            this.connectionIndex = connectionIndex;
            this.increaseIsGood = increaseIsGood;
            this.arrow = arrow;
            previousConnectionDelta = 0f;
        }
    }

    private void OnEnable()
    {
        if (PlayerManager.i.currentPlayer == null) return;
        uiGrid = new NodeGrid(gridSize);
        if (playerNodes == null)
        {
            playerNodes = PlayerManager.i.currentPlayer.GetComponent<PlayerNodes>();
            Init();
        }
    }

    private void Init()
    {
        List<int> currentAddedNodes = new List<int>();
        foreach (Node node in playerNodes.nodes) currentAddedNodes.Add(node.id);
        if (currentAddedNodes.Count > 0) AddNodes(currentAddedNodes);
        playerNodes.onNodeAdd += AddNodes;
        playerNodes.onNodeRemove += RemoveNodes;
        playerNodes.onNodeValueChange += ValueUpdate;
    }

    private void AddNodes(List<int> nodeIds)
    {
        foreach (int nodeId in nodeIds)
        {
            Debug.Log("added node of id: " + nodeId);
            CreateNode(nodeId);
        }
        AdjustLayout();
    }

    private void RemoveNodes(List<int> nodeIds)
    {
        foreach (int nodeId in nodeIds)
        {
            RemoveNode(nodeId);
        }
        AdjustLayout();
    }
    
    private void CreateNode(int id)
    {
        Node addedNode = playerNodes.GetNode(id);
        // Node object
        GameObject nodeObject = Instantiate(nodePrefab, contentTransform);
        NodeInfo nodeInfo = playerNodes.GetNodeInfo(addedNode.infoIndex);
        PhysNode pNode = nodeObject.GetComponent<PhysNode>();
        pNode.Init(nodeInfo);
        // Arrows
        List<ConnectionUI> connections = new List<ConnectionUI>();
        foreach (var connection in addedNode.connections)
        {
            GameObject arrowObject = Instantiate(arrowPrefab, arrowHolder);
            Node connectedNode = playerNodes.GetNode(connection.Key);
            NodeInfo connectedNodeInfo = playerNodes.GetNodeInfo(connectedNode.infoIndex);
            ConnectionUI cUI = new ConnectionUI(connection.Key, connection.Value, connectedNodeInfo.highIsGood, arrowObject.GetComponent<PhysArrow>());
            connections.Add(cUI);
        }
        uiConnections.Add(id, connections);
        uiObjects.Add(id, pNode);
    }

    private void AdjustLayout()
    {
        if (playerNodes.nodes.Count == 0) return;
        NodeGrid newUIGrid = CalculateNodeLayout();
        foreach (var kvp in newUIGrid.placedNodes)
        {
            // Arrow connection code
            if (uiConnections.ContainsKey(kvp.Key))
            {
                foreach (ConnectionUI cUI in uiConnections[kvp.Key])
                {
                    Debug.Log("initing arrow");
                    cUI.arrow.Init(GetContentLocation(kvp.Value), 
                        GetContentLocation(newUIGrid.placedNodes[cUI.connectedNodeId]));
                }
            }
            // Node move code
            if (uiGrid.placedNodes.ContainsKey(kvp.Key))
            {
                if (!uiGrid.placedNodes[kvp.Key].Equals(kvp.Value)) continue; // if same loc, don't move the node
            }
            MoveNode(kvp.Key, kvp.Value);
        }
        uiGrid = newUIGrid;
    }

    private void MoveNode(int id, Vector2Int location)
    {
        Vector2 newPos = GetContentLocation(location);
        GameObject nodeObject = uiObjects[id].gameObject;
        Transform nodeTransform = nodeObject.transform;
        nodeTransform.localPosition = newPos;
    }

    private NodeGrid CalculateNodeLayout()
    {
        NodeGrid output = new NodeGrid(gridSize);
        Node healthNode = playerNodes.GetNode(hpText);
        output.PlaceNode(healthNode.id, output.GetCenter());
        List<int> placeOrder = GetNodePlaceOrder(healthNode.id);
        foreach (int nodeId in placeOrder)
        {
            if (output.NodePlaced(nodeId)) continue;
            Node currentNode = playerNodes.GetNode(nodeId);
            // Get ids of every connection of this node to calculate the closest nodes
            List<int> connectionIds = new List<int>();
            foreach (var kvp in currentNode.connections) connectionIds.Add(kvp.Key);
            // Calculate closeness
            List<Vector2Int> placedNodes = GetClosestNodes(output, connectionIds);
            if (placedNodes.Count == 0) continue;
            if (placedNodes.Count == 1)
            {
                output.PlaceNode(nodeId, placedNodes[0]);
                continue;
            }
            // Calculate symmetry
            placedNodes = GetSymmetricalNodes(output, placedNodes);
            if (placedNodes.Count == 1)
            {
                output.PlaceNode(nodeId, placedNodes[0]);
                continue;
            }
            // Top to bottom, left to right
            Vector2Int finalPlacedNode = GetFirstNode(output, placedNodes);
            output.PlaceNode(nodeId, finalPlacedNode);
        }
        return output;
    }

    /// <summary>
    /// Gets the nodes closest to the specified connections
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="connections"></param>
    /// <returns></returns>
    private List<Vector2Int> GetClosestNodes(NodeGrid grid, List<int> connections)
    {
        List<Vector2Int> connectionLocations = new List<Vector2Int>();
        // Get every location from the node ids in connnections list
        foreach (int nodeId in connections)
        {
            if (!grid.NodePlaced(nodeId)) continue; // If not placed, continue
            connectionLocations.Add(grid.placedNodes[nodeId]);
        }
        if (connectionLocations.Count == 0) connectionLocations.Add(grid.GetCenter()); // Add center if disconnected
        // Find the tiles with the lowest closeness value
        float closestDistance = Mathf.Infinity;
        List<Vector2Int> closestLocations = new List<Vector2Int>();
        // Iterate over every tile
        for (int x = 0; x < grid.gridSize.x; x++)
        {
            for (int y = 0; y < grid.gridSize.y; y++)
            {
                Vector2Int currentLocation = new Vector2Int(x, y);
                if (grid.TileOccupied(currentLocation)) continue; // If this place is blocked, don't include in calculation
                float currentCloseness = 0f;
                foreach (Vector2Int location in connectionLocations)
                {
                    currentCloseness += Vector2Int.Distance(currentLocation, location);
                }
                // If there is a new closest distance, set closest and reset the loc list
                if (currentCloseness < closestDistance)
                {
                    closestDistance = currentCloseness;
                    closestLocations = new List<Vector2Int>() { currentLocation };
                    continue;
                }
                // If equal, add to the current loc list
                if (Mathf.Abs(currentCloseness - closestDistance) <= 0.01f)
                {
                    closestLocations.Add(currentLocation);
                }
            }
        }
        return closestLocations;
    }

    /// <summary>
    /// Gets the node placement locations which will lead to the most symmetry
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="nodes"></param>
    /// <returns></returns>
    private List<Vector2Int> GetSymmetricalNodes(NodeGrid grid, List<Vector2Int> nodes)
    {
        List<Vector2Int> highestNodes = new List<Vector2Int>(nodes);
        int highestSymmetry = 0;
        foreach (Vector2Int node in nodes)
        {
            // Skip if occupied
            if (grid.TileOccupied(node))
            {
                if (highestSymmetry == 0) highestNodes.Remove(node); // if symmetry is 0, then its still the initial list value, so remove it
                continue;
            }
            // Update highest symmetry stuff
            int currentSymmetry = grid.GetLocationSymmetry(node);
            if (currentSymmetry > highestSymmetry)
            {
                highestSymmetry = currentSymmetry;
                highestNodes = new List<Vector2Int>() { node };
            }
        }
        return highestNodes;
    }

    /// <summary>
    /// Gets the first node top to bottom left to right
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="nodes"></param>
    /// <returns></returns>
    private Vector2Int GetFirstNode(NodeGrid grid, List<Vector2Int> nodes)
    {
        Vector2Int bestNode = new Vector2Int(grid.gridSize.x, -1);
        foreach (Vector2Int node in nodes)
        {
            if (node.x < bestNode.x)
            {
                bestNode = node;
                continue;
            }
            if (node.x > bestNode.x) continue;
            if (node.y > bestNode.y)
            {
                bestNode.y = node.y;
            }
        }
        return bestNode;
    }

    /// <summary>
    /// Gets the placement order of nodes, starting from the specified start node id
    /// </summary>
    /// <param name="startNodeId"></param>
    /// <returns></returns>
    private List<int> GetNodePlaceOrder(int startNodeId)
    {
        List<int> disconnectedNodes = new List<int>(); // Nodes that aren't connected to anything
        List<int> discoveredNodes = new List<int>() { startNodeId }; 
        foreach (Node node in playerNodes.nodes)
        {
            foreach (var connection in node.connections)
            {
                // If we are pointing at a discovered node add it to discovered nodes and break
                if (discoveredNodes.Contains(connection.Key))
                {
                    discoveredNodes.Add(node.id);
                    break;
                }
            }
            // If we have iterated over every connection and found nothing, or if connection amount = 0, disconnected
            disconnectedNodes.Add(node.id);
        }
        // Add disconnected nodes to node order
        foreach (int nodeId in disconnectedNodes) discoveredNodes.Add(nodeId);
        return discoveredNodes;
    }

    private void RemoveNode(int id)
    {
        if (!uiObjects.ContainsKey(id)) return;
        Destroy(uiObjects[id]);
        // Destroy connection stuff
        if (uiConnections.ContainsKey(id))
        {
            foreach (ConnectionUI cUI in uiConnections[id])
            {
                Destroy(cUI.arrow.gameObject);
            }
            uiConnections.Remove(id);
        }
        uiObjects.Remove(id);
    }

    private Vector2 GetContentLocation(Vector2Int gridLocation)
    {
        Vector2 output = new Vector2(
            (gridLocation.x - uiGrid.GetCenter().x) * cellSize.x, 
            (gridLocation.y - uiGrid.GetCenter().y) * cellSize.y);
        return output;
    }

    private void ValueUpdate(Node node)
    {
        uiObjects[node.id].SetStatusText(node.value);
        NodeInfo nodeInfo = playerNodes.GetNodeInfo(node.infoIndex);
        if ((nodeInfo.criticalDisplayRange.x <= node.value) && (nodeInfo.criticalDisplayRange.y >= node.value))
        {
            Debug.LogError("displaying");
            onInfoSend?.Invoke(node.id);
        }
        // Updates the arrow uis
        List<ConnectionUI> cUIs = new List<ConnectionUI>();
        for (int i = 0; i < uiConnections[node.id].Count; i++)
        {
            ConnectionUI cUI = uiConnections[node.id][i];
            ConnectionInfo info = playerNodes.GetConnectionInfo(cUI.connectionIndex);
            float connectionDelta = info.effectLevels.Evaluate(node.value / 100f);
            if (!cUI.increaseIsGood) connectionDelta *= -1f;
            if (connectionDelta > 0f && (!(cUI.previousConnectionDelta > 0f)))
            {
                Debug.Log("starting postitive");
                cUI.arrow.SetDotted(false);
                cUI.arrow.SetTrianglesActive(true);
                cUI.arrow.StartFlash(increaseColor, 1.2f);
                cUI.arrow.SetTriangleText("+");
            }
            else if (connectionDelta < 0f && (!(cUI.previousConnectionDelta < 0f)))
            {
                Debug.Log("starting negative");
                cUI.arrow.SetDotted(false);
                cUI.arrow.SetTrianglesActive(true);
                cUI.arrow.StartFlash(decreaseColor, 3.2f);
                cUI.arrow.SetTriangleText("-");
            }
            else if (connectionDelta == 0 && (cUI.previousConnectionDelta != 0f))
            {
                Debug.Log("starting dotted");
                cUI.arrow.SetDotted(true);
                cUI.arrow.SetTrianglesActive(false);
                cUI.arrow.StopFlash();
            }
            cUI.previousConnectionDelta = connectionDelta;
            cUIs.Add(cUI);
        }
        uiConnections[node.id] = cUIs;
    }
}
