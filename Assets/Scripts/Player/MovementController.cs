using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

/*
 * FIX WALL RUNNING
 */

public enum MovementState
{ 
    Idle,
    Crouch,
    Running,
    Sliding,
    Jumping,
    WallRunning
}

[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(PlayerCrouch))]
[RequireComponent(typeof(PlayerWallRun))]
[RequireComponent(typeof(PlayerSlide))]
[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    // Child Objects
    [SerializeField] Transform c_CameraRoot; // Camera root object
    [SerializeField] private MovementState _MoveState;

    Rigidbody _rb; // Player's rigidbody

    KeyCode _jumpKey = KeyCode.Space;
    KeyCode _crouchKey = KeyCode.LeftControl;

    private PlayerSlide c_Slide;
    private PlayerJump c_Jump;
    private PlayerCrouch c_Crouch;
    private PlayerWallRun c_WallRun;

    public bool isGrounded { get; private set; } // If the player is on the floor
    public bool ControlsLocked { private get; set; } // If the players controls are locked

    [SerializeField] private float _playerSpeed = 1f; // How fast the player moves

    /*
     * CONSTRUCTOR
     */

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        c_Crouch = GetComponent<PlayerCrouch>();
        c_Jump = GetComponent<PlayerJump>();
        c_Slide = GetComponent<PlayerSlide>();
        c_WallRun = GetComponent<PlayerWallRun>();

    }

    // Start is called before the first frame update
    void Start()
    {
        _rb.mass = 1;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _MoveState = MovementState.Idle;

        ControlsLocked = false;
    }

    /*
     * UNITY METHODS
     */

    // Update Method
    void Update()
    {
        MovementHandler();
    }

    // Fixed Update Method
    private void FixedUpdate()
    {
        _rb.velocity = ClampVelocity(_rb.velocity, new Vector3(0f, -9.81f, 0f), 20);
    }

    // Function
    // Desc - Clamps the players velocity to make movement more consistent
    private Vector3 ClampVelocity(Vector3 velocity, Vector3 Gravity, float Clamp)
    {
        var grav = Vector3.Project(velocity, Gravity);
        var cla = velocity - grav;
        cla = Vector3.ClampMagnitude(cla, Clamp);
        return cla + grav;
    }

    /*
     * ORGANISER FUNCTIONS
     */

    // Organiser Function
    // Desc - Perfoms all movement functions
    void MovementHandler()
    {
        // if (_rb == null) throw new ArgumentException("Player does not contain rigidbody");

        _MoveState = MovementState.Idle;

        if(_MoveState == MovementState.Idle)
        {
            if(_rb.velocity.magnitude > 1f)
            {
                _MoveState = MovementState.Running;
            }
            else
            {
                _MoveState = MovementState.Idle;
            }
        }

        if (ControlsLocked != true)
        {
            InputManager(); // Basic Movement
        }


        CheckIfGrounded(); // Grounded Check

        c_Crouch.Crouch();
        c_Slide.Slide();
        c_WallRun.WallRun();
        c_Jump.JumpIN();

        // JumpIN(_jumpKey); // Jump Input
    }

    /*
     * CHECKS
     */

    // Function
    // Desc - Checks if the player is touching the ground
    private void CheckIfGrounded()
    {
        RaycastHit hit;

        // Fires a raycast towards the ground, if it does not hit anything it returns is Grounded as false
        if (Physics.Raycast(this.transform.position, Vector3.down, out hit, transform.localScale.y / 2 + 0.6f))
        {
            isGrounded = true;
            Debug.DrawLine(this.transform.position, hit.point, Color.red);
            return;
        }

        isGrounded = false;
    }
    /*
     * INPUT FUNCTIONS
     */

    // Function
    // Desc - Checks for player input and moves the player accordingly
    void InputManager()
    {
        PlayerMovement();


    }

    void PlayerMovement()
    {
        // Gets keyboard input
        float Horizontal = Input.GetAxisRaw("Horizontal");
        float Vertical = Input.GetAxisRaw("Vertical");

        // Converts Input into useable movement vectors
        Vector3 movement = new Vector3(Horizontal, 0f, Vertical);

        movement.Normalize();

        movement *= _playerSpeed;

        movement = transform.TransformDirection(movement);

        // Applies movement to player rigidbody
        _rb.velocity = new Vector3(movement.x, _rb.velocity.y, movement.z);
    }

    void JumpInput()
    {

    }

    public MovementState CurrentState()
    {
        return _MoveState;
    }

    public void SetMovestate(MovementState mState)
    {
        _MoveState = mState;
    }

    /*
    private void wallClimb()
    {
        switch (isGrounded)
        {
            case false:
                break;
            default:
                return;
        }

        RaycastHit hit;

        if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, 1.2f))
        {
            if (!Input.GetKey(KeyCode.W))
            {
                return;
            }

            if (hit.point.y < hit.collider.transform.position.y + hit.collider.transform.localScale.y / 2 - 1)
            {
                return;
            }

            this.transform.position = new Vector3(transform.position.x, hit.collider.transform.position.y + hit.collider.transform.localScale.y / 2, transform.position.z);
        }
    }
    */
}