using Fusion;
using Fusion.Addons.Physics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static Fusion.NetworkBehaviour;

public class PlayerMovement : NetworkBehaviour//PunCallbacks
{
    [Header("Movement")]
    public float speed = 6f;
    public float movementMultiplier = 10f;
    public float sprintMultiplier = 1.67f;
    public float sprintStaminaRegenCooldown = 0f;
    public float crouchMultiplier = 0.4f;
    public float sprintStaminaConsumption = 20f;
    public float groundDrag = 6f;
    [Networked] public bool canMove { get; set; } = true;

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
    public int groundChecks = 8;
    public float groundCheckDegree = 30f;
    [Networked] public bool isGrounded { get; set; }

    [Header("Crouching")]
    public float crouchJumpMultiplier = 0.6f;
    public float crouchTime = .2f;
    public AnimationCurve crouchCurve;
    [Networked] public bool isCrouching { get; set; } = false;
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
    public float fallDamageMultiplier = 3f;
    public float mercyDistance = 5f;
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
    public Transform itemComponentHolder;

    public MovementEvent OnLeap;

    public delegate void MovementEvent();

    PlayerManager playerManager;
    //PhotonView view;
    NetworkObject no;
    PlayerStats stats;
    PlayerInput playerInput;
    Rigidbody rb;
    CameraBobbing bobbing;
    CameraShake shake;
    RunnerManager runnerManager;
    CameraManager camManager;
    float sprintGain = 1f;
    float crouchMinus = 1f;
    //float jumpTimer = 0f;
    [HideInInspector] public float horizontalMovement;
    [HideInInspector] public float verticalMovement;
    float previousYVel;
    float peakYPosition;
    float unCastDistance;
    [HideInInspector] [Networked] public bool isMoving { get; set; }
    [HideInInspector] [Networked] public bool isSprinting { get; set; }
    bool previousGrounded = true;
    public bool sprintPressed = false;
    bool initialized = false;
    RaycastHit slopeHit;
    Vector3 moveDirection;
    Vector3 slopeDirection;
    Vector3 groundCheckOffset;
    Vector3 standOffset;
    Vector3 crouchOffset;
    IEnumerator currentCamLerp;

    // Client Feedback
    [Networked]
    int jumpCount { get; set; }
    int lastJump;

    ChangeDetector changeDetector;

    private void Awake()
    {
        groundCheckOffset = groundCheck.localPosition;
        standOffset = standPos.localPosition;
        crouchOffset = crouchPos.localPosition;
        no = gameObject.GetComponent<NetworkObject>();
        playerManager = FindFirstObjectByType<PlayerManager>();
        playerInput = gameObject.GetComponent<PlayerInput>();
        rb = gameObject.GetComponent<Rigidbody>();
        gameObject.GetComponent<Player>().Init += Init;
        stats = gameObject.GetComponent<PlayerStats>();
        runnerManager = FindFirstObjectByType<RunnerManager>();
        stepRayUpper.localPosition = new Vector3(stepRayUpper.localPosition.x, stepHeight, stepRayUpper.localPosition.z);
        unCastDistance = uncrouchCastUpper.position.y - uncrouchCastLower.position.y;
        rb.freezeRotation = true;
    }

    public void Init() // Client side initialization
    {
        initialized = true;
        if (!no.HasInputAuthority) 
        { 
            Destroy(playerInput);
            headAim.parent = cameraPosition; // Sets the headaim position to be synced on all clients
            headAim.localPosition = new Vector3(0f, 0f, 1f);
            return; 
        }
        CameraMovement cm = playerManager.camTransform.GetComponent<CameraMovement>();
        camManager = playerManager.camTransform.GetComponent<CameraManager>();
        cm.player = transform;
        cm.orientation = orientation;
        cm.headAim = headAim;
        bobbing = playerManager.camBobbing;
        shake = playerManager.camShake;
        playerManager.camTransform.GetComponent<CameraManager>().SetTrackedFPS(rb, cameraPosition);
        InputManager inputManager = FindFirstObjectByType<InputManager>();
        inputManager.onMove += OnMove;
        inputManager.onSprint += OnSprint;
        inputManager.onCrouch += OnCrouch;
        inputManager.onJump += OnJump;
    }

    private void Update()
    {
        if (!initialized) return;
        UpdateAnimatorParemeters();
        if (canMove) UpdateAnimatorSpeed();
        if (!runnerManager.nRunner.IsServer && !no.HasInputAuthority) return; // If not the server or the inputter, then return
        //if (jumpTimer > 0f && isGrounded) jumpTimer -= Time.deltaTime;
        if (no.HasInputAuthority) Bobbing();
    }

    private void LateUpdate()
    {
        
    }

    public override void Spawned()
    {
        airSpeed = speed;
        jumpCount = lastJump;
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        if (IsProxy)
        {
            ProxyCrouchLerp();
        }
    }

    public override void Render()
    {
        if (jumpCount > lastJump) // Sees if the jump count has changed, then sets lastJump to the jump count
        {
            if (!isCrouching)
            {
                animator.Play("Jump");
                if (!HasInputAuthority) SoundManager.i.Play3D("Jump", groundCheck.position);
            }
            lastJump = jumpCount;
        }

        foreach (var change in changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(isCrouching):
                    ChangeCrouchHitboxes(isCrouching);
                    ProxyCrouchLerp(); // Changes the stat position for proxies
                    break;
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        SetGrounded();
        Sprint();
        //if (HasInputAuthority && camManager != null) camManager.SetPositions();
    }

    public void SetGrounded()
    {
        RaycastHit info;
        Vector3 origin = rb.position + groundCheckOffset;
        bool hit = Physics.Raycast(origin, Vector3.down, out info, groundedRadius, environmentMask);
        if (hit)
        {
            // If the surface exceeds 45 degrees, the player will not be grounded.
            if (Vector3.Dot(Vector3.up, info.normal) > 0.707106781f)
            {
                isGrounded = true;
                return;
            }
        }
        // Cast in every direction
        Vector3 castDirection = Quaternion.Euler(groundCheckDegree, 0f, 0f) * Vector3.down;
        float castRotation = 360f / groundChecks;
        for (int i = 0; i < groundChecks; i++)
        {
            RaycastHit cInfo;
            bool cHit = Physics.Raycast(origin, castDirection, out cInfo, groundedRadius, environmentMask);
            Debug.DrawLine(origin, origin + castDirection * groundedRadius, Color.blue, 1f);
            castDirection = Quaternion.Euler(0f, castRotation, 0f) * castDirection;
            if (!cHit) continue;
            if (Vector3.Dot(Vector3.up, cInfo.normal) > 0.707106781f)
            {
                isGrounded = true;
                return;
            }
        }
        isGrounded = false;
    }

    public void GroundSim()
    {
        ControlDrag();
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

    public void Freeze(bool freezeVelocity = false)
    {
        canMove = false;
        isMoving = false;
        ExitCrouch();
        isSprinting = false;
        if (freezeVelocity)
        {
            rb.velocity = Vector3.zero;
            peakYPosition = transform.position.y;
        }
    }

    public void Unfreeze()
    {
        canMove = true;
    }

    private void OnSprint(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            runnerManager.sprint = true;
            return;
        }
        runnerManager.sprint = false;
    }

    private void OnJump()
    {
        runnerManager.jump = true; // Set the jump button to true to be networked
    }

    private void OnMove(InputValue iv)
    {
        if (!canMove) return;
        Vector2 mv = iv.Get<Vector2>();
        runnerManager.moveDirection = mv;
    }

    private void OnCrouch(InputValue iv)
    {
        if (iv.Get<float>() == 1f)
        {
            if (!canMove) return;
            runnerManager.crouch = true;
            return;
        }
        runnerManager.crouch = false;
    }

    public void Jump()
    {
        if (!canMove) return;
        if (!canJump) return;
        if (!isGrounded) return;
        //if (!(jumpTimer <= 0f)) return;
        if (!stats.ConsumeStamina(jumpStaminaConsumption)) return;
        if (!Runner.IsResimulation && HasInputAuthority) ClientJump();
        if (Runner.IsServer) jumpCount++;
        if (isCrouching) rb.AddForce(Vector3.up * jumpHeight * crouchJumpMultiplier, ForceMode.Impulse);
        if (!isCrouching)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
        //jumpTimer = jumpCooldown;
        SetAirSpeed();
    }

    public void ClientJump()
    {
        SoundManager.i.Play3D("Jump", groundCheck.position);
        shake.StartShake(jumpShake.shakeProperties);
        OnLeap?.Invoke();
    }

    // Sets the airspeed depending on the movement state of the player
    private void SetAirSpeed()
    {
        if (isSprinting) airSpeed = speed * sprintMultiplier;
        if (!isSprinting) airSpeed = speed;
        if (isCrouching) airSpeed = speed * crouchMultiplier;
    }

    public void JumpAnimation()
    {
        animator.Play("Jump");
    }

    public void EnterCrouch()
    {
        if (isCrouching) return;
        isCrouching = true;
        if (isSprinting)
        {
            sprintPressed = false;
            isSprinting = false;
        }
        crouchMinus = crouchMultiplier;
        if (!Runner.IsResimulation) StartCamLerp(cameraPosition, crouchPos, crouchOffset);
    }

    public void ExitCrouch()
    {
        CheckCrouchExit();
    }

    void CheckCrouchExit()
    {
        if (!isCrouching) return;
        if (!CanUncrouch()) return;
        crouchMinus = 1f;
        if (!Runner.IsResimulation) StartCamLerp(cameraPosition, standPos, standOffset);
        isCrouching = false;
    }

    void StartCamLerp(Transform from, Transform to, Vector3 toRb)
    {
        if (currentCamLerp != null)
        {
            StopCoroutine(currentCamLerp);
        }
        currentCamLerp = LerpCamPos(from, to, toRb);
        StartCoroutine(currentCamLerp);
    }

    /// <summary>
    /// Takes a transform and lerps it to a local player position, toRb is the networked rigidbody position
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    IEnumerator LerpCamPos(Transform from, Transform to, Vector3 toRb)
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
            Vector3 newPos = Vector3.Lerp(newFrom.position, to.position, lerpPercent); // Transform position and rb position are different
            Vector3 newPosHolder = Vector3.Lerp(newFrom.position, rb.position + toRb, lerpPercent);
            cameraPosition.position = newPos;
            itemComponentHolder.position = newPosHolder;
            lerpTime += Time.deltaTime;
        }
        cameraPosition.position = to.position;
        itemComponentHolder.position = rb.position + toRb;
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

    void ChangeCrouchHitboxes(bool enabled) 
    {
        SetColliders(crouchingColliders, enabled);
        SetColliders(standingColliders, !enabled);
    }

    void ProxyCrouchLerp()
    {
        if (!IsProxy) return;
        if (isCrouching)
        {
            StartCamLerp(cameraPosition, crouchPos, crouchOffset);
        } 
        else
        {
            StartCamLerp(cameraPosition, standPos, standOffset);
        }
    }

    void SetColliders(Collider[] colliders, bool isActive)
    {
        foreach (Collider c in colliders)
        {
            c.enabled = isActive;
        }
    }

    public void SetDirection(Vector2 direction)
    {
        horizontalMovement = direction.x;
        verticalMovement = direction.y;
        moveDirection = new Vector3(direction.x, 0f, direction.y);
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
        
    }

    void OnLand()
    {
        float fallDistance = peakYPosition - transform.position.y;

        if (fallDistance < 0.4f) return;
        if (fallDistance > mercyDistance)
        {
            shake?.StartShake(hardFall.shakeProperties);
            stats.Damage(fallDistance * fallDamageMultiplier);
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, groundCheck.up * -1f, out hit, Mathf.Infinity, (int)environmentMask))
            {
                SoundMaterial sma = hit.transform.GetComponent<SoundMaterial>();
                if (sma == null) return;
                string mat = sma.GetSMat(hit.textureCoord);
                SoundManager.i.Play3D(mat + "LandHard", transform.position);
            }
        }
        else
        {
            shake?.StartShake(softFall.shakeProperties);
            RaycastHit hit;
            if (Physics.Raycast(groundCheck.position, groundCheck.up * -1f, out hit, Mathf.Infinity, (int)environmentMask))
            {
                SoundMaterial sma = hit.transform.GetComponent<SoundMaterial>();
                if (sma == null) return;
                string mat = sma.GetSMat(hit.textureCoord);
                SoundManager.i.Play3D(mat + "LandSoft", groundCheck.position);
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
                    ExitCrouch();
                }
                stats.staminaRegenCooldown = sprintStaminaRegenCooldown;
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

    public void CapAirVelocity()
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

    public void Inputs()
    {
        moveDirection = orientation.forward * verticalMovement + orientation.right * horizontalMovement; // Turns horizontal and vertical movement variables into a movedirection vector
        slopeDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal); // For slope movement
    }

    public void MovePlayer()
    {
        if (!canMove) return;   
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

    public void SetIsMoving()
    {
        if (moveDirection != Vector3.zero)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }
    }

    void UpdateAnimatorSpeed()
    {
        if (isMoving)
        {
            animator.SetFloat("moveMultiplier", (speed / aniSpeedFactor));
        } 
        else
        {
            animator.SetFloat("moveMultiplier", 1f);
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

    public void StepClimb()
    {
        //if (OnSlope()) return; // May be changed later
        if (!isMoving) return;
        if (!isGrounded) return;
        RaycastHit hit;
        if (Physics.Raycast(stepRayLower.position, moveDirection, out hit, stepDistance, environmentMask))
        {
            if (hit.collider.tag == "No Step Climb") return; // if we are looking at terrain, don't do any step climbs
            bool upper = Physics.Raycast(stepRayUpper.position, moveDirection, stepDistance + .05f, environmentMask);
            if (!upper)
            {
                rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y + stepSmooth, rb.velocity.z);
            }
        }
    }
}
