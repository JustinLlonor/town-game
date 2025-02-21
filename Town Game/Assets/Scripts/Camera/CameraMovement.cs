using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    public LayerMask subInteractableMask;
    public float mouseSensitivity = 1f;
    public Transform player;
    public Transform orientation;
    public Transform headAim;
    public Transform observableCursor;
    public CameraBehaviourBase cameraState;
    private PlayerCameraMovement pCamState = new PlayerCameraMovement();
    private ObservableCameraMovement oCamState = new ObservableCameraMovement();

    Transform fpsTransform;
    NetworkRigidbody3D playerRb;
    Transform playerGFX;
    CursorManager cursorManager;
    CameraManager cameraManager;
    RunnerManager runnerManager;
    [HideInInspector] public float xRotation = 0f;
    [HideInInspector] public float yRotation = 0f;
    public bool canMove = true;
    bool settable = true;
    Vector2 cameraDelta;

    private void Awake()
    {
        cameraManager = FindAnyObjectByType<CameraManager>();
        cursorManager = FindFirstObjectByType<CursorManager>();
        runnerManager = FindFirstObjectByType<RunnerManager>();
        FindFirstObjectByType<PlayerManager>().OnInstantiatePlayer += AssignReferences;
        cameraManager.OnSwitchCameraMode += OnCameraModeChange;
        GameManager gm = FindFirstObjectByType<GameManager>();
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onCamera += GetCameraDelta;
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
        CameraLook();
    }

    void GetCameraDelta(InputValue iv)
    {
        cameraDelta = iv.Get<Vector2>() / 40f;
    }

    void EnableCanMove()
    {
        canMove = true;
        settable = true;
    }
    
    void CameraLook()
    {
        if (cameraState == null) return;
        cameraState.CameraLook(this, cameraManager, runnerManager);
    }

    public bool CursorLocked()
    {
        return cursorManager.isLocked;
    }

    public Vector2 GetMouseMovement()
    {
        float mouseX = cameraDelta.x * mouseSensitivity;
        float mouseY = cameraDelta.y * mouseSensitivity;
        return new Vector2(mouseX, mouseY);
    }

    public Vector3 GetMousePosition()
    {
        return Input.mousePosition;
    }

    void AssignReferences(GameObject player)
    {
        Debug.Log("Assigning References");
        playerRb = player.GetComponent<NetworkRigidbody3D>();
        PlayerMovement mv = player.GetComponent<PlayerMovement>();
        this.player = mv.graphics;
        orientation = mv.orientation;
        headAim = mv.headAim;
        cameraState = pCamState;

        yRotation = player.transform.eulerAngles.y;
    }

    void OnCameraModeChange(CameraManager.CameraMode mode)
    {
        if (!settable && mode == CameraManager.CameraMode.FirstPerson) return;
        canMove = (mode == CameraManager.CameraMode.FirstPerson); // Sets the canMove to true if it is first person
        if (mode == CameraManager.CameraMode.FirstPerson)
        {
            Cursor.visible = true;
            cursorManager.Lock();
            cameraState = pCamState;
        }
        if (mode == CameraManager.CameraMode.Observe)
        {
            //Cursor.visible = false;
            cursorManager.Unlock();
            cameraState = oCamState;
        }
        if (mode == CameraManager.CameraMode.Cinematic)
        {
            headAim.position = transform.position;
        }
    }
}
