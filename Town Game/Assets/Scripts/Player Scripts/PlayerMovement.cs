using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using Photon.Voice;

// THIS MOVEMENT SCRIPT IS TO BE REWRITTEN IN THE FUTURE
public class PlayerMovement : MonoBehaviourPunCallbacks
{
    public float mouseSensitivity = 1f;
    public float acceleration = 0.4f;
    public float initialVelocity = 1f;
    public float speed = 8f;
    public float jumpHeight = 5;
    public LayerMask environmentMask;
    public Transform groundCheck;
    public Animator animator;
    public Transform headAim;
    private PhotonView view;
    private Transform cam;
    private Rigidbody rb;
    private float netVel = 0f;
    private bool isGrounded;
    private bool isMoving;
    private float xRotation = 0f;
    private float yRotation = 0f;
    private Vector3 movementVector;

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
            Cursor.lockState = CursorLockMode.Locked;   
        }
    }

    private void Update()
    {
        if (!view.IsMine) return;

        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, environmentMask);

        if (Input.GetKeyDown("space"))
        {
            if (isGrounded)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
            }
        }

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);
        yRotation += mouseX;
        cam.eulerAngles = new Vector3(xRotation, cam.eulerAngles.y, 0);
        transform.eulerAngles = new Vector3(0, yRotation, 0);

        float inputY = Input.GetAxis("Vertical");
        float inputX = Input.GetAxis("Horizontal");

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

        headAim.position = cam.position + cam.forward;

        UpdateAnimatorParemeters();
    }

    void FixedUpdate()
    {
        if (movementVector != new Vector3(0, 0, 0))
        {
            rb.velocity = movementVector;
        }
    }

    void UpdateAnimatorParemeters()
    {
        animator.SetBool("isGrounded", isGrounded);
        animator.SetBool("isMoving", isMoving);
    }
}
