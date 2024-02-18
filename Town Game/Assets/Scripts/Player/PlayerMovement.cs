using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    public float speed = 6f;
    public float movementMultiplier = 10f;
    public float sprintMultiplier = 1.67f;
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
    public Transform orientation;
    PlayerManager playerManager;
    CursorManager cursorManager;
    PhotonView view;
    PlayerStats stats;
    Rigidbody rb;
    CameraBobbing bobbing;
    CameraShake shake;  
    float sprintGain = 1f;
    float jumpTimer = 0f;
    float horizontalMovement;
    float verticalMovement;
    float previousYVel;
    float peakYPosition;
    bool isMoving;
    bool isSprinting;
    bool previousGrounded = true;
    RaycastHit slopeHit;
    Vector3 moveDirection;
    Vector3 slopeDirection;
    private Controls _controls;

    private void Awake()
    {
        airSpeed = speed;
        view = gameObject.GetComponent<PhotonView>();
        playerManager = FindObjectOfType<PlayerManager>();
        cursorManager = FindObjectOfType<CursorManager>();
        if (!view.IsMine) Destroy(gameObject.GetComponent<PlayerInput>());
        _controls = new Controls();
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

    private new void OnEnable()
    {
        _controls.Enable();
    }

    private new void OnDisable()
    {
        _controls.Disable();
    }

    private void OnSprint() { }

    private void OnJump()
    {
        if (!canJump) return;
        if (!stats.ConsumeStamina(jumpStaminaConsumption)) return;
        if (!isGrounded) return;
        if (!(jumpTimer <= 0f)) return;
        shake.StartShake(jumpShake.shakeProperties);
        rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
        jumpTimer = jumpCooldown;
        SoundManager.instance.Play3D("Jump", groundCheck.position);
    }

    private void OnMove(InputValue iv)
    {
        Vector2 mv = iv.Get<Vector2>();
        horizontalMovement = mv.x;
        verticalMovement = mv.y;
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
    }

    void Sprint()
    {
        bool onSprintKey = _controls.BaseGameplay.Sprint.ReadValue<float>() > 0f;
        if (isMoving && onSprintKey && isGrounded)
        {
            if (stats.RateConsumeStamina(sprintStaminaConsumption))
            {
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
            rb.AddForce(moveDirection.normalized * speed * movementMultiplier * sprintGain, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeDirection.normalized * speed * movementMultiplier * sprintGain, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * speed * movementMultiplier * airHandling * sprintGain, ForceMode.Acceleration);
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
        if (OnSlope()) return; // May be changed later
        if (!isMoving) return;
        if (!isGrounded) return;
        if (Physics.Raycast(stepRayLower.position, moveDirection, stepDistance, environmentMask))
        {
            bool upper = Physics.Raycast(stepRayUpper.position, moveDirection, stepDistance + .05f, environmentMask);
            if (!upper)
            {
                rb.position -= new Vector3(0f, -stepSmooth, 0f);
            }
        }
    }
}
