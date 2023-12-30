using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

// IF IT WORKS IT WORKS 
public class PlayerMovement : MonoBehaviourPunCallbacks
{
    [Header("Movement")]
    public float mouseSensitivity = 1f;
    public float acceleration = 0.4f;
    public float initialVelocity = 1f;
    public float speed = 8f;
    public float jumpHeight = 5;
    public float airHandling = 2f;
    public bool canJump = true;
    public LayerMask environmentMask;
    public Transform groundCheck;
    public float groundedRadius = 0.2f;
    [Header("Animation")]
    public Collider movementCollider;
    public Animator animator;
    public float aniSpeedFactor = 2.5f;
    public Transform headAim;
    private PhotonView view;
    private Transform cam;
    private Rigidbody rb;
    // Private values
    private float netVel = 0f;
    private bool isGrounded;
    private bool previousGrounded;
    private bool isMoving;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector3 movementVector;
    private RaycastHit slopeHit;
    private Vector3 slopeVector;

    private void Awake()
    {
        cam = Camera.main.transform;
        view = gameObject.GetComponent<PhotonView>();
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (view.IsMine)
        {
            cam.parent = transform;
            cam.localPosition = new Vector3(0f, 1.637f, 0f);
            cam.localRotation = Quaternion.identity;
            Cursor.lockState = CursorLockMode.Locked;   
        }
    }

    private void Update()
    {
        if (!view.IsMine) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundedRadius, environmentMask);
        OnLand();
        OnJump();
        JumpControls();
        CameraLook();
        WASDMove();
        headAim.position = cam.position + cam.forward;
        UpdateAnimatorParemeters();
        UpdateAnimatorSpeed();
        slopeVector = Vector3.ProjectOnPlane(movementVector, slopeHit.normal);
        MovePlayer();
        AirCap();
        previousGrounded = isGrounded;
    }

    private void OnLand()
    {
        if (!previousGrounded && isGrounded)
        {
            movementVector.x = rb.velocity.x / 2f;
            movementVector.z = rb.velocity.z / 2f;
        }
    }

    private void OnJump()
    {
        if (previousGrounded && !isGrounded)
        {
            Vector3 newVel = rb.velocity;
            newVel.x = movementVector.x;
            newVel.z = movementVector.z;
            rb.velocity = newVel;
        }
    }

    private void AirCap()
    {
        if (!isGrounded)
        {
            if (new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude > speed)
            {
                float yVel = rb.velocity.y;
                rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z).normalized * speed;
                rb.velocity = new Vector3(rb.velocity.x, yVel, rb.velocity.z);
            }
        }
    }

    private bool OnSlope()
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

    private void FixedUpdate()
    {
        if (!isGrounded)
        {
            movementVector.y = 0f;
            rb.AddForce(movementVector * airHandling, ForceMode.Acceleration);
        }
        AirCap();
    }

    private void MovePlayer()
    {
        if (movementVector != Vector3.zero)
        {
            if (isGrounded && !OnSlope())
            {
                rb.position += movementVector * Time.deltaTime;
            }
            else if (isGrounded && OnSlope())
            {
                rb.position += slopeVector * Time.deltaTime;
            }
        }
    }

    private void JumpControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && canJump)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
            }
        }
    }

    private void CameraLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        yRotation += mouseX;
        cam.eulerAngles = new Vector3(xRotation, cam.eulerAngles.y, 0);
        transform.eulerAngles = new Vector3(0, yRotation, 0);
    }

    private void WASDMove()
    {
        float inputY = Input.GetAxisRaw("Vertical");
        float inputX = Input.GetAxisRaw("Horizontal");
        Vector3 verticalVector = transform.forward * inputY;
        Vector3 horizontalVector = transform.right * inputX;
        if (inputY != 0f || inputX != 0f)
        {
            netVel += acceleration * Time.deltaTime;
            netVel = Mathf.Clamp(netVel, initialVelocity, speed);
            isMoving = true;
        }
        else
        {
            netVel = 0f;
            isMoving = false;
        }
        movementVector = (verticalVector + horizontalVector).normalized * netVel;
        movementVector.y = rb.velocity.y;
    }

    private void UpdateAnimatorParemeters()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isMoving", isMoving);
    }

    private void UpdateAnimatorSpeed()
    {
        if (isMoving)
        {
            animator.SetFloat("moveMultiplier", speed / aniSpeedFactor);
        } else
        {
            animator.SetFloat("moveMultiplier", 1f);
        }
    }
}
