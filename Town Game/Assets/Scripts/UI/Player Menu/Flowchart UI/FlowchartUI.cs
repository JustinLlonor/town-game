using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowchartUI : MonoBehaviour
{
    public Vector2Int gridSize;
    public Vector2 cellSize;
    public Transform contentTransform;
    public GameObject nodePrefab;
    public GameObject arrowPrefab;
    public NodeGrid uiGrid;
    public Dictionary<int, GameObject> uiObjects = new Dictionary<int, GameObject>();
    PlayerNodes nodes;

    public struct NodeGrid
    {
        public Vector2Int gridSize;
        private Dictionary<int, Vector2Int> placedNodes;

        public NodeGrid(Vector2Int gridSize)
        {
            this.gridSize = gridSize;
            placedNodes = new Dictionary<int, Vector2Int>();
        }
    }

    private void OnEnable()
    {
        if (PlayerManager.i.currentPlayer == null) return;
        uiGrid = new NodeGrid(gridSize);
        if (nodes == null)
        {
            nodes = PlayerManager.i.currentPlayer.GetComponent<PlayerNodes>();
            Init();
        }
    }

    private void PlaceNodeOnGrid(int id, Vector2Int location)
    {
        Node addedNode = nodes.GetNode(id);
        GameObject nodeObject = Instantiate(nodePrefab, contentTransform);
    }

    private Vector2 GetContentLocation(Vector2Int gridLocation)
    {
        return Vector2.zero;
    }

    private void Init()
    {

    }
}
