using Pinwheel.Poseidon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairManager : MonoBehaviour
{
    public static CrosshairManager instance;
    public Texture2D[] crosshairs;
    public List<CrosshairLayer> crosshairLayers = new List<CrosshairLayer>() { };
    public RawImage ri;
    int previousCrosshair = -1;

    [System.Serializable]
    public class CrosshairLayer
    {
        public int crosshair;
        public int priority;

        public CrosshairLayer(int crosshair, int priority)
        {
            this.crosshair = crosshair;
            this.priority = priority;
        } 
    }

    private void Awake()
    {
        CrosshairManager.instance = this;
    }

    private void Update()
    {
        DisplayCrosshair();
    }

    /// <summary>
    /// Adds a crosshair layer. A crosshair with a low priority number will be shown over a crosshair with a high priority number.
    /// </summary>
    /// <param name="crosshairIndex">Index of the crosshair, seen in crosshair texture array</param>
    /// <param name="priority">Priority of the crosshair</param>
    public void AddCrosshair(int crosshairIndex,  int priority)
    {
        int i = 0;
        foreach (CrosshairLayer layer in crosshairLayers)
        {
            if (layer.priority < priority)
            {
                i++;
                continue;
            }
            break;
        }
        crosshairLayers.Insert(i, new CrosshairLayer(crosshairIndex, priority));
    }

    /// <summary>
    /// Removes all crosshairs with the specified priority
    /// </summary>
    /// <param name="priority"></param>
    public void RemoveCrosshair(int priority)
    {
        CrosshairLayer removedLayer = null;
        foreach (CrosshairLayer layer in crosshairLayers)
        {
            if (layer.priority == priority)
            {
                removedLayer = layer;
                break;
            }
        }
        if (removedLayer != null) crosshairLayers.Remove(removedLayer);
    }

    void DisplayCrosshair()
    {
        if (crosshairLayers.Count == 0)
        {
            previousCrosshair = -1;
            ri.color = new Color(1, 1, 1, 0);
            return;
        }

        int index = crosshairLayers[0].crosshair;
        if (previousCrosshair == index) return;
        previousCrosshair = index;
        Texture2D cTex = crosshairs[index];

        ri.texture = cTex;
        ri.color = new Color(1, 1, 1, 1);
    }
}
