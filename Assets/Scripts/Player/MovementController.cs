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

    [SerializeField] private CharacterController _characterController;

    [SerializeField] private float _playerSpeed = 1f; // How fast the player moves
    [SerializeField] private float _jumpForce; // How high the player jumps
    [SerializeField] private float _baseJumpForce; // Static version of jump force (Used for calculations)

    public bool isGrounded { get; private set; } // If the player is on the floor
    [SerializeField] private int _jumpIteration;
    [SerializeField] private int _jumpIterationMaximum = 100;
    [SerializeField] private float _baseJumpMulitiplier = 1.0f;

    public bool ControlsLocked { private get; set; } // If the players controls are locked

    [SerializeField] private int _WallRunLength = 100;
    private int _UpForceCount;
    [SerializeField] private float _wallRunRotationMaximum = 10.0f;
    [SerializeField] private float _cWallRunRotation;
    [SerializeField] private float _WallRunCameraRotationSpeed = 0.5f;
    private int _wallJumpLock;
    [SerializeField] private float _wallRunJumpPower = 1.5f;

    float initialForce = 0;

    /*
     * CONSTRUCTOR
     */

    // Start is called before the first frame update
    void Start()
    {
        if (TryGetComponent<Rigidbody>(out Rigidbody rb) == false)
        {
            this.AddComponent<Rigidbody>();
            _rb = GetComponent<Rigidbody>();
        }
        else
        {
            _rb = rb;
        }

        c_Crouch = GetComponent<PlayerCrouch>();
        c_Jump = GetComponent<PlayerJump>();
        c_Slide = GetComponent<PlayerSlide>();
        c_WallRun = GetComponent<PlayerWallRun>();

        _rb.mass = 1;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _MoveState = MovementState.Idle;

        ControlsLocked = false;

        _UpForceCount = _WallRunLength;
        _cWallRunRotation = _wallRunRotationMaximum;
        _wallJumpLock = 1;

        _jumpForce = _baseJumpForce;

        Debug.Log("Running");
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
        c_Slide.Slide();
        Crouch();
        WallRun();
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

    public MovementState CurrentState()
    {
        return _MoveState;
    }

    public void SetMovestate(MovementState mState)
    {
        _MoveState = mState;
    }

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

    // Function that allows the player to crouch
    private void Crouch()
    {
        // Returns if player is not grounded
        if (_MoveState != MovementState.Crouch)
        {
            switch (isGrounded)
            {
                case true:
                    break;
                default:
                    return;
            }

            // Returns if the player is currently sliding
            switch (_MoveState)
            {
                case MovementState.Idle:
                    break;
                default:
                    return;
            }
        }

        // If the player presses C, lower camera into a lower position
        if (Input.GetKeyDown(_crouchKey))
        {
            c_CameraRoot.localPosition = new Vector3(c_CameraRoot.transform.localPosition.x, 0.3f, c_CameraRoot.transform.localPosition.z);

            _MoveState = MovementState.Crouch;
        }

        // If the player releases C, heighten Camera
        if (Input.GetKeyUp(_crouchKey))
        {
            c_CameraRoot.localPosition = new Vector3(c_CameraRoot.transform.localPosition.x, 0.6f, c_CameraRoot.transform.localPosition.z);

            _MoveState = MovementState.Idle;
        }
    }

    /*
     * ----------
     *  WALL RUN
     * ----------
     */

    void WallRun()
    {
        _cWallRunRotation = Mathf.Clamp(_cWallRunRotation, 0.0f, _wallRunRotationMaximum * 2);
        RaycastHit raycastHit;

        if (Physics.Raycast(this.transform.position, transform.right, out raycastHit, 0.8f) || Physics.Raycast(this.transform.position, -transform.right, out raycastHit, 0.8f))
        {
            Debug.DrawLine(this.transform.position, raycastHit.point, Color.red);

            if (!raycastHit.transform.CompareTag("Wall"))
            {
                return;
            }

            switch (isGrounded)
            {
                case false:
                    break;
                default:
                    return;
            }

            if (Input.GetKey(KeyCode.W))
            {
                if (Input.GetKey(KeyCode.D))
                {
                    _cWallRunRotation += _WallRunCameraRotationSpeed;
                    // c_CameraRoot.Rotate(Vector3.forward, Mathf.Abs(_cWallRunRotation-10), Space.Self);
                }
                else if (Input.GetKey(KeyCode.A))
                {
                    _cWallRunRotation -= _WallRunCameraRotationSpeed;
                    // c_CameraRoot.Rotate(-Vector3.forward, Mathf.Abs(_cWallRunRotation-10), Space.Self);
                }

                if (_UpForceCount > 0)
                {
                    _rb.AddForce(transform.up * 1);
                    _rb.AddForce(transform.forward * 5);
                    _UpForceCount--;
                }

                c_Jump.groundedBypass = true;
                _MoveState = MovementState.WallRunning;
            }
            else
            {
                if (_cWallRunRotation == _wallRunRotationMaximum)
                {
                    return;
                }
                if (_cWallRunRotation < _wallRunRotationMaximum)
                {
                    _cWallRunRotation += _WallRunCameraRotationSpeed;
                }
                if (_cWallRunRotation > _wallRunRotationMaximum)
                {
                    _cWallRunRotation -= _WallRunCameraRotationSpeed;
                }

                _UpForceCount = _WallRunLength;

                c_Jump.jumpCount = 1;
            }

            // c_CameraRoot.Rotate(Vector3.forward, _cWallRunRotation - _wallRunRotationMaximum, Space.Self);

            // Add force pushing the player up over a set duration
            // Similiar to how the slide works until the player clicks jump
        }
        else
        {
            // _cWallRunRotation -= _WallRunCameraRotationSpeed;

            _UpForceCount = _WallRunLength;

            if (_cWallRunRotation == _wallRunRotationMaximum)
            {
                return;
            }
            if (_cWallRunRotation < _wallRunRotationMaximum)
            {
                _cWallRunRotation += _WallRunCameraRotationSpeed;
            }
            if (_cWallRunRotation > _wallRunRotationMaximum)
            {
                _cWallRunRotation -= _WallRunCameraRotationSpeed;
            }

            c_Jump.jumpCount = 1;
        }

        c_CameraRoot.Rotate(Vector3.forward, _cWallRunRotation - _wallRunRotationMaximum, Space.Self);
    }
}