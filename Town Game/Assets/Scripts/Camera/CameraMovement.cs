using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float mouseSensitivity = 1f;
    public Transform player;
    public Transform orientation;
    public Transform headAim;

    Transform fpsTransform;
    CursorManager cursorManager;
    CameraManager cameraManager;
    RunnerManager runnerManager;
    [HideInInspector] public float xRotation = 0f;
    [HideInInspector] public float yRotation = 0f;
    public bool canMove = true;
    bool settable = true;

    private void Awake()
    {
        cameraManager = FindAnyObjectByType<CameraManager>();
        cursorManager = FindObjectOfType<CursorManager>();
        runnerManager = FindObjectOfType<RunnerManager>();
        FindObjectOfType<PlayerManager>().OnInstantiatePlayer += AssignReferences;
        cameraManager.OnSwitchCameraMode += OnCameraModeChange;
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm == null)
        {
            canMove = true;
            return;
        }
        settable = false;
        Invoke("EnableCanMove", 1f);
    }

    private void Update()
    {
        if (player == null) return;
        CameraLook();
    }

    void EnableCanMove()
    {
        canMove = true;
        settable = true;
    }
    
    void CameraLook()
    {
        if (cameraManager.isTransitioning) return;
        if (!canMove) return;
        if (!cursorManager.isLocked) return;
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        yRotation += mouseX;
        transform.eulerAngles = new Vector3(xRotation, yRotation, 0f);
        orientation.eulerAngles = new Vector3(0, yRotation, 0);
        player.eulerAngles = orientation.eulerAngles;
        headAim.position = transform.position + transform.forward;
        runnerManager.orientation = yRotation;
    }

    void AssignReferences(GameObject player)
    {
        Debug.Log("Assigning References");
        PlayerMovement mv = player.GetComponent<PlayerMovement>();
        this.player = mv.graphics;
        orientation = mv.orientation;
        headAim = mv.headAim;

        yRotation = player.transform.eulerAngles.y;
    }

    void OnCameraModeChange(CameraManager.CameraMode mode)
    {
        if (!settable && mode == CameraManager.CameraMode.FirstPerson) return;
        canMove = (mode == CameraManager.CameraMode.FirstPerson);
        if (mode == CameraManager.CameraMode.Cinematic)
        {
            headAim.position = transform.position;
        }
    }
}
