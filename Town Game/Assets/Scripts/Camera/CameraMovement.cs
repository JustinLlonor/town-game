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
    public float primaryDown = 0f;
    public ZTilt zTilt;
    private PlayerCameraMovement pCamState = new PlayerCameraMovement();
    private ObservableCameraMovement oCamState = new ObservableCameraMovement();
    public Transform lockedPlayer = null;
    public float lockLerpTime = 2f;
    public AnimationCurve lockCurve;
    public bool lockX = false; // if this is true, the camera movement on the x axis is locked
    public float lockedDelta;

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
    float lockLerpTimer = 0f;
    float startX = 0f;
    float startY = 0f;

    private void Awake()
    {
        cameraManager = FindAnyObjectByType<CameraManager>();
        cursorManager = FindFirstObjectByType<CursorManager>();
        runnerManager = FindFirstObjectByType<RunnerManager>();
        FindFirstObjectByType<PlayerManager>().onInstantiatePlayer += AssignReferences;
        cameraManager.onSwitchCameraMode += OnCameraModeChange;
        GameManager gm = FindFirstObjectByType<GameManager>();
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onCamera += GetCameraDelta;
        inputManager.onPrimaryObserve += GetObservablePrimary;
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
        if (lockedPlayer != null)
        {
            LockLerp();
            return;
        }
        CameraLook();
    }

    void GetCameraDelta(InputValue iv)
    {
        cameraDelta = iv.Get<Vector2>() / 30f;
    }

    void GetObservablePrimary(InputValue iv)
    {
        primaryDown = iv.Get<float>();
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
            Cursor.visible = false;
            cursorManager.Lock();
            cameraState = pCamState;
        }
        if (mode == CameraManager.CameraMode.Observe)
        {
            Cursor.visible = true;
            cursorManager.Unlock();
            cameraState = oCamState;
        }
        if (mode == CameraManager.CameraMode.Cinematic)
        {
            headAim.position = transform.position;
        }
    }

    public void ResetLerpTimer()
    {
        lockLerpTimer = 0f;
        startX = xRotation;
        startY = yRotation;
    }

    void LockLerp()
    {
        if (lockLerpTimer > 1f)
        {
            Vector3 targetD = (lockedPlayer.position - transform.position).normalized;
            Quaternion targetR = Quaternion.LookRotation(targetD);
            Vector3 targetE = targetR.eulerAngles;
            yRotation = targetE.y;
            xRotation = targetE.x;

            runnerManager.orientation = yRotation;
            runnerManager.camOrientation = xRotation;
            return;
        }
        lockLerpTimer += Time.deltaTime/lockLerpTime;
        transform.eulerAngles = new Vector3(xRotation, yRotation, 0f);
        Vector3 targetDirection = (lockedPlayer.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Vector3 targetEuler = targetRotation.eulerAngles;
        yRotation = Mathf.LerpAngle(startY, targetEuler.y, lockCurve.Evaluate(lockLerpTimer));
        xRotation = Mathf.LerpAngle(startX, targetEuler.x, lockCurve.Evaluate(lockLerpTimer));

        runnerManager.orientation = yRotation;
        runnerManager.camOrientation = xRotation;
    }

    public void LockX()
    {
        lockX = true;
        lockedDelta = 0f;
        zTilt.canTurn = false;
    }

    public float GetLockedDelta()
    {
        float returnedDelta = lockedDelta;
        lockedDelta = 0f;
        return returnedDelta;
    }

    public void UnlockX()
    {
        lockX = false;
        zTilt.canTurn = true;
    }
}
