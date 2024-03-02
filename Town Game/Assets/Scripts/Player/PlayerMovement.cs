using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using JetBrains.Annotations;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    public float speed = 6f;
    public float movementMultiplier = 10f;
    public float sprintMultiplier = 1.67f;
    public float crouchMultiplier = 0.4f;
    public float sprintStaminaConsumption = 20f;
    public float groundDrag = 6f;
    
    [Header("Aerial")]
    public float jumpHeight = 3;
    public float jumpCooldown = 0.5f;
    public float jumpStaminaConsumption = 20f;
    public bool canJump = true;
    public float airHandling = 0.4f;
    float airSpeed = 2.7f;
    public LayerMask environmentMask;
    public Transform groundCheck;
    public Shake jumpShake;
    public float groundedRadius = 0.2f;
    public bool isGrounded = true;

    [Header("Crouching")]
    public float crouchJumpMultiplier = 0.6f;
    public float crouchTime = .2f;
    public AnimationCurve crouchCurve;
    public bool isCrouching = false;
    public float playerRadius;
    public Transform uncrouchCastUpper;
    public Transform uncrouchCastLower;
    public Collider[] standingColliders;
    public Collider[] crouchingColliders;

    [Header("Stairs")]
    public float stepHeight = 0.3f;
    public float stepSmooth = .1f;
    public float stepDistance = .2f;
    public Transform stepRayLower;
    public Transform stepRayUpper;

    [Header("Fall")]
    public float fallDamageMultiplier = 1f;
    public float mercyDistance = 3f;
    public Shake softFall;
    public Shake hardFall;

    [Header("Animation")]
    public Collider movementCollider;
    public Animator animator;
    public float aniSpeedFactor = 2.5f;
    public Transform headAim;

    [Header("Camera")]
    public Transform graphics;
    public Transform cameraPosition;
    public Transform standPos;
    public Transform crouchPos;
    public Transform orientation;
    PlayerManager playerManager;
    CursorManager cursorManager;
    PhotonView view;
    PlayerStats stats;
    Rigidbody rb;
    CameraBobbing bobbing;
    CameraShake shake;  
    float sprintGain = 1f;
    float crouchMinus = 1f;
    float jumpTimer = 0f;
    float horizontalMovement;
    float verticalMovement;
    float previousYVel;
    float peakYPosition;
    float unCastDistance;
    bool isMoving;
    bool isSprinting;
    bool previousGrounded = true;
    bool sprintPressed = false;
    RaycastHit slopeHit;
    Vector3 moveDirection;
    Vector3 slopeDirection;
    IEnumerator currentCamLerp;
    IEnumerator currentCrouchExit;

    private void Awake()
    {
        airSpeed = speed;
        view = gameObject.GetComponent<PhotonView>();
        playerManager = FindObjectOfType<PlayerManager>();
        cursorManager = FindObjectOfType<CursorManager>();
        if (!view.IsMine) Destroy(gameObject.GetComponent<PlayerInput>());
        if (!view.IsMine) return;
        stats = gameObject.GetComponent<PlayerStats>();
        rb = gameObject.GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        CameraMovement cm = playerManager.camTransform.GetComponent<CameraMovement>();
        cm.player = graphics;
        cm.orientation = orientation;
        cm.headAim = headAim;
        bobbing = playerManager.camBobbing;
        shake = playerManager.camShake;
        playerManager.camTransform.GetComponent<CamMove>().camPos = cameraPosition;
        stepRayUpper.localPosition = new Vector3(stepRayUpper.localPosition.x, stepHeight, stepRayUpper.localPosition.z);
        unCastDistance = uncrouchCastUpper.position.y - uncrouchCastLower.position.y;
    }

    private void Update()
    {
        if (!view.IsMine) return;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundedRadius, environmentMask);
        Inputs();
        ControlDrag();
        slopeDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
        if (jumpTimer > 0f && isGrounded) jumpTimer -= Time.deltaTime;
        Sprint();
        Bobbing();
        UpdateAnimatorParemeters();
        UpdateAnimatorSpeed();
        if (!previousGrounded && isGrounded)
        {
            OnLand();
        }
        if (previousGrounded && !isGrounded)
        {
            OnAir();
        }
        previousGrounded = isGrounded;
        Fall();
    }

    private void FixedUpdate()
    {
        if (!view.IsMine) return;
        MovePlayer();
        CapAirVelocity();
        StepClimb();
    }

    private void OnSprint(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            sprintPressed = true;
            return;
        }
        sprintPressed = false;
    }

    private void OnJump()
    {
        if (!canJump) return;
        if (!stats.ConsumeStamina(jumpStaminaConsumption)) return;
        if (!isGrounded) return;
        if (!(jumpTimer <= 0f)) return;
        if (isCrouching) rb.AddForce(transform.up * jumpHeight * crouchJumpMultiplier, ForceMode.Impulse);
        if (!isCrouching)
        {
            animator.Play("Jump");
            view.RPC("JumpAnimation", RpcTarget.Others);
            rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
            SoundManager.instance.Play3D("Jump", groundCheck.position);
        }
        shake.StartShake(jumpShake.shakeProperties);
        jumpTimer = jumpCooldown;
    }

    [PunRPC]
    public void JumpAnimation()
    {
        animator.Play("Jump");
    }

    private void OnMove(InputValue iv)
    {
        Vector2 mv = iv.Get<Vector2>();
        horizontalMovement = mv.x;
        verticalMovement = mv.y;
    }

    private void OnCrouch(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            EnterCrouch();
            return;
        }
        ExitCrouch();
    }

    void EnterCrouch()
    {
        if (isCrouching) return;
        if (isSprinting)
        {
            sprintPressed = false;
            isSprinting = false;
        }
        EnableCrouchHitboxes();
        view.RPC("EnableCrouchHitboxes", RpcTarget.Others);
        crouchMinus = crouchMultiplier;
        StartCamLerp(cameraPosition, crouchPos);
        isCrouching = true;
    }

    void ExitCrouch()
    {
        if (!isCrouching) return;
        if (currentCrouchExit != null)
        {
            StopCoroutine(currentCrouchExit);
        }
        currentCrouchExit = CheckCrouchExit();
        StartCoroutine(currentCrouchExit);
    }

    void StartCamLerp(Transform from, Transform to)
    {
        if (currentCamLerp != null)
        {
            StopCoroutine(currentCamLerp);
        }
        currentCamLerp = LerpCamPos(from, to);
        StartCoroutine(currentCamLerp);
    }

    IEnumerator LerpCamPos(Transform from, Transform to)
    {
        Transform newFrom = Instantiate(from.gameObject, from.parent).transform;
        StartCoroutine(LifeTimer(newFrom.gameObject));
        float lerpTime = 0f;
        float lerpMax = crouchTime;
        while (lerpTime < crouchTime)
        {
            yield return null;
            float lerpPercent = lerpTime / lerpMax;
            lerpPercent = crouchCurve.Evaluate(lerpPercent);
            Vector3 newPos = Vector3.Lerp(newFrom.position, to.position, lerpPercent);
            cameraPosition.position = newPos;
            lerpTime += Time.deltaTime;
        }
        cameraPosition.position = to.position;
    }

    IEnumerator CheckCrouchExit()
    {
        while (!CanUncrouch())
        {
            yield return null;
        }
        DisableCrouchHitboxes();
        view.RPC("DisableCrouchHitboxes", RpcTarget.Others);
        crouchMinus = 1f;
        StartCamLerp(cameraPosition, standPos);
        isCrouching = false;
    }

    IEnumerator LifeTimer(GameObject obj)
    {
        float timer = 0f;
        while (timer < crouchTime)
        {
            yield return null;
            timer += Time.deltaTime;
        }
        Destroy(obj);
    }

    [PunRPC]
    void EnableCrouchHitboxes() 
    {
        SetColliders(crouchingColliders, true);
        SetColliders(standingColliders, false);
    }

    [PunRPC]
    void DisableCrouchHitboxes()
    {
        SetColliders(crouchingColliders, false);
        SetColliders(standingColliders, true);
    }

    void SetColliders(Collider[] colliders, bool isActive)
    {
        foreach (Collider c in colliders)
        {
            c.enabled = isActive;
        }
    }

    bool CanUncrouch()
    {
        Ray ray = new Ray(uncrouchCastLower.position, Vector3.up);
        bool canUncrouch = !Physics.SphereCast(ray, playerRadius-.001f, unCastDistance, environmentMask);
        return canUncrouch;
    }
     
    void Fall() 
    {
        if (isGrounded)
        {
            peakYPosition = transform.position.y;
            return;
        }
        
        if (previousYVel >= 0f && rb.velocity.y < 0f)
        {
            peakYPosition = transform.position.y;
        }
        previousYVel = rb.velocity.y;
    }

    void OnAir()
    {
        if (isSprinting) airSpeed = speed * sprintMultiplier;
        if (!isSprinting) airSpeed = speed;
        if (isCrouching) airSpeed = speed * crouchMultiplier;
    }

    void OnLand()
    {
        float fallDistance = peakYPosition - transform.position.y;

        if (fallDistance < 0.4f) return;
        if (fallDistance > mercyDistance)
        {
            shake.StartShake(hardFall.shakeProperties);
            stats.Damage(fallDistance * fallDamageMultiplier, false);
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, groundCheck.up * -1f, out hit, Mathf.Infinity, (int)environmentMask))
            {
                SoundMaterial sma = hit.transform.GetComponent<SoundMaterial>();
                if (sma == null) return;
                string mat = sma.GetSMat(hit.textureCoord);
                SoundManager.instance.Play3D(mat + "LandHard", transform.position);
            }
        }
        else
        {
            shake.StartShake(softFall.shakeProperties);
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, groundCheck.up * -1f, out hit, Mathf.Infinity, (int)environmentMask))
            {
                SoundMaterial sma = hit.transform.GetComponent<SoundMaterial>();
                if (sma == null) return;
                string mat = sma.GetSMat(hit.textureCoord);
                SoundManager.instance.Play3D(mat + "LandSoft", groundCheck.position);
            }
        }
    }

    void Bobbing()
    {
        if (!isGrounded)
        {
            bobbing.isBobbing = false;
            return;
        }
        bobbing.isSprinting = isSprinting;
        bobbing.isBobbing = isMoving;
        bobbing.isCrouching = isCrouching;
    }

    void Sprint()
    {
        if (isMoving && sprintPressed && isGrounded)
        {
            if (stats.RateConsumeStamina(sprintStaminaConsumption))
            {
                if (isCrouching)
                {
                    if (!CanUncrouch()) return;
                }
                ExitCrouch();
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
                if (stats.staminaCooldown <= 0f)
                {
                    stats.staminaCooldown = 1.5f;
                }
            }
        } 
        else
        {
            isSprinting = false;
        }

        stats.canRegenStamina = !isSprinting;

        if (isSprinting)
        {
            sprintGain = sprintMultiplier;
        }
        else
        {
            sprintGain = 1f;
        }
        
    }

    void CapAirVelocity()
    {
        if (!isGrounded)
        {
            Vector3 checkVel = rb.velocity;
            checkVel.y = 0f;
            if (checkVel.magnitude > airSpeed)
            {
                checkVel = checkVel.normalized * airSpeed;
                rb.velocity = new Vector3(checkVel.x, rb.velocity.y, checkVel.z);
            }
        }
    }

    void MovePlayer()
    {
        if (isGrounded && !OnSlope())
        {
            rb.AddForce(moveDirection.normalized * speed * movementMultiplier * sprintGain * crouchMinus, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeDirection.normalized * speed * movementMultiplier * sprintGain * crouchMinus, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * speed * movementMultiplier * airHandling * sprintGain * crouchMinus, ForceMode.Acceleration);
        }
    }

    void Inputs()
    {
        if (!cursorManager.isLocked) return;
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement;
    }

    void ControlDrag()
    {
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 0f;
        }
    }

    void UpdateAnimatorParemeters()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isRunning", isSprinting);
        animator.SetBool("isCrouching", isCrouching);
    }

    void UpdateAnimatorSpeed()
    {
        if (moveDirection != Vector3.zero)
        {
            isMoving = true;
        } else
        {
            isMoving = false;
        }
        if (isMoving)
        {
            //animator.SetFloat("moveMultiplier", speed / aniSpeedFactor);
        } 
        else
        {
            //animator.SetFloat("moveMultiplier", 1f);
        }
    }
    
    bool OnSlope()
    {
        if (Physics.Raycast(groundCheck.position, Vector3.down, out slopeHit, 0.5f))
        {
            if (slopeHit.normal != Vector3.up)
            {
                return true;
            }
        }
        return false;
    }

    void StepClimb()
    {
        //if (OnSlope()) return; // May be changed later
        if (!isMoving) return;
        if (!isGrounded) return;
        if (Physics.Raycast(stepRayLower.position, moveDirection, stepDistance, environmentMask))
        {
            bool upper = Physics.Raycast(stepRayUpper.position, moveDirection, stepDistance + .05f, environmentMask);
            if (!upper)
            {
                Debug.Log("Stepping");
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }
    }
}
