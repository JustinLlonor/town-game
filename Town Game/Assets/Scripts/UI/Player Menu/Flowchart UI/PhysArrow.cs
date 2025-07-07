using Mono.Cecil.Cil;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhysArrow : MonoBehaviour
{
    /// <summary>
    /// How many units each arrow is apart
    /// </summary>
    public float arrowSpacing = 40f;
    public float arrowSpeed = 10f;
    public GameObject triangleObject;
    public RectTransform lineTransform;
    public Transform triangleHolder;
    public Image lineImage;
    public Sprite dottedLineSprite;
    public bool autoInitialize = false;
    public bool trianglesActive = false;
    public Color dottedColor;
    public Color undottedColor;
    public bool flashEnabled;
    public float flashSpeed;
    public Color flashColor;
    float finalHeight;
    float progress;

    public List<MaskableGraphic> graphics = new List<MaskableGraphic>();
    List<Transform> triangles = new List<Transform>();

    private void Awake()
    {
        if (autoInitialize)
        {
            Init();
        }
    }

    private void Update()
    {
        FlashColor();
    }

    private void LateUpdate()
    {
        if (triangles.Count == 0) return;
        if (!trianglesActive) return;
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

    /// <summary>
    /// Initialize without calculations
    /// </summary>
    public void Init()
    {
        graphics.Clear();
        graphics.Add(lineImage);
        SetTrianglesActive(trianglesActive);
        // Transform stuff
        float height = lineTransform.sizeDelta.y;
        // Triangles
        if (triangleHolder.childCount > 0)
        {
            foreach (Transform transform in triangleHolder)
            {
                Destroy(transform.gameObject);
            }
        }
        int triangleCount = Mathf.CeilToInt(height / arrowSpacing);
        for (int i = 0; i < triangleCount; i++)
        {
            GameObject newTriangle = Instantiate(triangleObject, triangleHolder);
            newTriangle.transform.localPosition = new Vector2(0f, i * arrowSpacing);
            Transform textTransform = newTriangle.transform.GetChild(0);
            textTransform.eulerAngles = Vector3.zero;
            triangles.Add(newTriangle.transform);
            graphics.Add(newTriangle.GetComponent<MaskableGraphic>());
        }
        finalHeight = height;
    }

    /// <summary>
    /// Initialize with a start and end point
    /// </summary>
    /// <param name="startLocation">Start local pos</param>
    /// <param name="endLocation">End local pos</param>
    public void Init(Vector2 startLocation, Vector2 endLocation)
    {
        graphics.Clear();
        graphics.Add(lineImage);
        // Transform stuff
        transform.localPosition = startLocation;
        float height = Vector2.Distance(startLocation, endLocation);
        lineTransform.sizeDelta = new Vector2(lineTransform.sizeDelta.x, height);
        // Triangles
        if (triangleHolder.childCount > 0)
        {
            foreach (Transform transform in triangleHolder)
            {
                Destroy(transform.gameObject);
            }
        }
        int triangleCount = Mathf.CeilToInt(height / arrowSpacing);
        for (int i = 0; i < triangleCount; i++)
        {
            GameObject newTriangle = Instantiate(triangleObject, triangleHolder);
            newTriangle.transform.localPosition = new Vector2(0f, i * arrowSpacing);
            Transform textTransform = newTriangle.transform.GetChild(0);
            textTransform.eulerAngles = Vector3.zero;
            triangles.Add(newTriangle.transform);
            graphics.Add(newTriangle.GetComponent<MaskableGraphic>());
        }
        finalHeight = height;
    }

    public void SetTrianglesActive(bool active)
    {
        triangleHolder.gameObject.SetActive(active);
        trianglesActive = active;
    }

    public void SetTriangleText(string text)
    {
        foreach (Transform triangle in triangles)
        {
            TextMeshProUGUI triangleText = triangle.GetComponentInChildren<TextMeshProUGUI>();
            triangleText.text = text;
        }
    }

    public void SetDotted(bool dotted)
    {
        if (dotted)
        {
            lineImage.color = dottedColor;
            lineImage.sprite = dottedLineSprite;
        }
        else
        {
            lineImage.color = undottedColor;
            lineImage.sprite = null;
        }
    }

    public void SetColor(Color color)
    {
        foreach (MaskableGraphic graphic in graphics)
        {
            graphic.color = color;
        }
    }

    private void FlashColor()
    {
        if (!flashEnabled) return;
        progress += Time.deltaTime * flashSpeed;
        if (progress > Mathf.PI) progress -= Mathf.PI;
        float evaluation = (Mathf.Cos(progress) + 1f) / 2f;
        Color newColor = Color.Lerp(undottedColor, flashColor, evaluation);
        SetColor(newColor);
    }
}
