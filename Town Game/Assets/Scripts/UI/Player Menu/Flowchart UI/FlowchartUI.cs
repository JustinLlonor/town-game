using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowchartUI : MonoBehaviour
{
    public Vector2Int gridSize;
    public Vector2 cellSize;
    public GameObject nodePrefab;
    public GameObject arrowPrefab;
    private List<int> placedNodes = new List<int>();
    PlayerNodes nodes;

    private void OnEnable()
    {
        if (PlayerManager.i.currentPlayer == null) return;
        if (nodes == null)
        {
            nodes = PlayerManager.i.currentPlayer.GetComponent<PlayerNodes>();
            Init();
        }
    }

    private void Init()
    {

    }
}
