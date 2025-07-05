using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysArrow : MonoBehaviour
{
    /// <summary>
    /// How many units each arrow is apart
    /// </summary>
    public float arrowSpacing = 40f;
    public float arrowSpeed = 10f;
    public GameObject triangleObject;
    public RectTransform lineTransform;
    public bool autoInitialize = false;
    float finalHeight;

    List<Transform> triangles = new List<Transform>();

    private void Awake()
    {
        if (autoInitialize)
        {
            Init();
        }
    }

    /// <summary>
    /// Initialize without calculations
    /// </summary>
    public void Init()
    {
        // Transform stuff
        float height = lineTransform.sizeDelta.y;
        // Triangles
        int triangleCount = Mathf.CeilToInt(height / arrowSpacing);
        for (int i = 0; i < triangleCount; i++)
        {
            GameObject newTriangle = Instantiate(triangleObject, lineTransform);
            newTriangle.transform.localPosition = new Vector2(0f, i * arrowSpacing);
            Transform textTransform = newTriangle.transform.GetChild(0);
            textTransform.eulerAngles = Vector3.zero;
            triangles.Add(newTriangle.transform);
        }
        finalHeight = height;
    }

    /// <summary>
    /// Initialize with a start and end point
    /// </summary>
    /// <param name="startLocation"></param>
    /// <param name="endLocation"></param>
    public void Init(Vector2 startLocation, Vector2 endLocation)
    {
        // Transform stuff
        transform.position = startLocation;
        float height = Vector2.Distance(startLocation, endLocation);
        lineTransform.sizeDelta = new Vector2(lineTransform.sizeDelta.x, height);
        // Triangles
        int triangleCount = Mathf.CeilToInt(height / arrowSpacing);
        for (int i = 0; i < triangleCount; i++)
        {
            GameObject newTriangle = Instantiate(triangleObject, transform);
            newTriangle.transform.localPosition = new Vector2(0f, i * arrowSpacing);
            Transform textTransform = newTriangle.transform.GetChild(0);
            textTransform.eulerAngles = Vector3.zero;
            triangles.Add(newTriangle.transform);
        }
        finalHeight = height;
    }

    private void LateUpdate()
    {
        if (triangles.Count == 0) return;
        float moveDistance = Time.deltaTime * arrowSpeed;
        foreach (Transform triangle in triangles)
        {
            if (triangle.localPosition.y >= finalHeight) continue;
            triangle.localPosition += Vector3.up * moveDistance;
            if (triangle.localPosition.y >= finalHeight)
            {
                triangle.localPosition = Vector3.up * finalHeight;
                triangle.gameObject.SetActive(false);
            }
        }
        if (triangles[0].transform.localPosition.y > arrowSpacing)
        {
            float newY = triangles[0].transform.localPosition.y - arrowSpacing;
            int finalTriangle = triangles.Count - 1;
            Transform movedTransform = triangles[finalTriangle];
            movedTransform.gameObject.SetActive(true);
            movedTransform.localPosition = new Vector3(0f, newY);
            triangles.RemoveAt(finalTriangle);
            triangles.Insert(0, movedTransform);
        }
    }
}
