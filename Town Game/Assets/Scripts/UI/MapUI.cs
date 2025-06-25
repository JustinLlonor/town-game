using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapUI : MonoBehaviour
{
    public float maxZoom = 100f;
    public float minZoom = 3f;
    public float minPanResistance = 5f;
    public float maxPanResistance = 50f;
    public float panResistance;
    public AnimationCurve panCurve;
    public Minimap map;
    public RectTransform controlBounds;
    bool init = false;
    Transform camTransform;
    InputManager inputManager;
    private float currentZoom = 10f;
    Vector2 cameraDelta;
    Vector3 currentPosition;
    bool isDragging = false;
    bool canControl;

    public void Init()
    {
        init = true;
        camTransform = Camera.main.transform;
        inputManager = FindAnyObjectByType<InputManager>();
        inputManager.onMapZoom += OnZoom;
        inputManager.onMapDrag += OnPan;
        //inputManager.onCamera += OnCamera;
        CalculatePanResistance();
    }

    private void OnEnable()
    {
        if (!init) Init();
        map.SetPosition(camTransform.position);
        currentPosition = camTransform.position;
        inputManager.onCamera += OnCamera;
    }

    private void OnDisable()
    {
        inputManager.onCamera -= OnCamera;
    }

    private void Update()
    {
        if (isDragging) MoveMap();
        canControl = RectTransformUtility.RectangleContainsScreenPoint(controlBounds, Input.mousePosition);
    }

    private void OnZoom(InputValue zoom)
    {
        if (!canControl) return;
        float value = zoom.Get<float>();
        if (value == 0f) return;
        if (value > 0f)
        {
            ZoomOut(value/120f);
        }
        if (value < 0f)
        {
            ZoomIn(-value/120f);
        }
        CalculatePanResistance();

    }

    public void SetZoom(float zoom)
    {
        currentZoom = zoom;
        map.SetZoom(currentZoom);
    }

    private void ZoomOut(float delta)
    {
        currentZoom -= delta;
        if (currentZoom < minZoom) currentZoom = minZoom;
        map.SetZoom(currentZoom);
    }

    private void ZoomIn(float delta)
    {
        currentZoom += delta;
        if (currentZoom > maxZoom) currentZoom = maxZoom;
        map.SetZoom(currentZoom);
    }

    private void OnPan(InputValue iv)
    {
        isDragging = iv.Get<float>() == 1f;
    }

    private void OnCamera(InputValue iv)
    {
        cameraDelta = iv.Get<Vector2>();
    }

    private void MoveMap()
    {
        if (!canControl) return;
        currentPosition += new Vector3(-cameraDelta.x, 0f, -cameraDelta.y) / panResistance;
        map.SetPosition(currentPosition);
    }

    private void CalculatePanResistance()
    {
        float progress = (currentZoom - minZoom) / (maxZoom - minZoom);
        progress = panCurve.Evaluate(progress);
        panResistance = Mathf.Lerp(maxPanResistance, minPanResistance, progress);
    }
}
