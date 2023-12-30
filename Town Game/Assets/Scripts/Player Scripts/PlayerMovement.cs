using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Rendering;

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
    public bool canJump = true;
    public float jumpStaminaConsumption = 20f;
    public float airHandling = 0.4f;
    public float airSpeed = 2.7f;
    public LayerMask environmentMask;
    public Transform groundCheck;
    public float groundedRadius = 0.2f;

    [Header("Stairs")]
    public float stepHeight = 0.1f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

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
    PhotonView view;
    PlayerStats stats;
    Rigidbody rb;
    bool isGrounded;
    bool isMoving;
    float horizontalMovement;
    float verticalMovement;
    RaycastHit slopeHit;
    Vector3 moveDirection;
    Vector3 slopeDirection;

    private void Awake()
    {
        view = gameObject.GetComponent<PhotonView>();
        playerManager = FindObjectOfType<PlayerManager>();
        if (!view.IsMine) return;
        rb = gameObject.GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        CameraMovement cm = playerManager.camTransform.GetComponent<CameraMovement>();
        cm.player = graphics;
        cm.orientation = orientation;
        cm.headAim = headAim;
        playerManager.camTransform.GetComponent<CamMove>().camPos = cameraPosition;
    }

    private void Update()
    {
        if (!view.IsMine) return;
        isGrounded = Physics.CheckSphere(groundCheck.position, groundedRadius, environmentMask);
        MyInput();
        ControlDrag();
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }
        slopeDirection = Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
        UpdateAnimatorParemeters();
        UpdateAnimatorSpeed();
    }

    private void FixedUpdate()
    {
        if (!view.IsMine) return;
        MovePlayer();
        CapAirVelocity();
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
            rb.AddForce(moveDirection.normalized * speed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isGrounded && OnSlope())
        {
            rb.AddForce(slopeDirection.normalized * speed * movementMultiplier, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * speed * movementMultiplier * airHandling, ForceMode.Acceleration);
        }
    }

    void Jump()
    {
        if (!canJump) return;
        rb.AddForce(transform.up * jumpHeight, ForceMode.Impulse);
    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

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
            animator.SetFloat("moveMultiplier", speed / aniSpeedFactor);
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
}
