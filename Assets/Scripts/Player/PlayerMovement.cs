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

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // Child Objects
    [SerializeField] Transform c_CameraRoot; // Camera root object

    Rigidbody _rb; // Player's rigidbody

    KeyCode _jumpKey = KeyCode.Space;
    KeyCode _crouchKey = KeyCode.LeftControl;

    [SerializeField] private CharacterController _characterController;

    [SerializeField] private float _playerSpeed = 1f; // How fast the player moves
    [SerializeField] private float _jumpForce; // How high the player jumps
    [SerializeField] private float _baseJumpForce; // Static version of jump force (Used for calculations)

    private bool _isGrounded; // If the player is on the floor
    private bool _isWallRunning;
    [SerializeField] private bool _currentlyJumping; // If the player is currently jumping
    [SerializeField] private int _jumpIteration;
    [SerializeField] private int _jumpIterationMaximum = 100;
    private int _jumpCount;

    private bool _GroundCheckBypass;
    private float _jumpMultiplier;
    [SerializeField] private float _baseJumpMulitiplier = 1.0f;

    [SerializeField] private MovementState _MoveState;

    private bool _lockControl; // If the players controls are locked

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

        _rb.mass = 1;
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _MoveState = MovementState.Idle;

        _lockControl = false;

        _UpForceCount = _WallRunLength;
        _isWallRunning = false;
        _cWallRunRotation = _wallRunRotationMaximum;
        _wallJumpLock = 1;

        _jumpForce = _baseJumpForce;
        _jumpMultiplier = _baseJumpMulitiplier;
        _currentlyJumping = false;
        _jumpCount = 1;

        _GroundCheckBypass = false;
        _jumpMultiplier = 1.0f;

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

        AttributeReseter();

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

        if (_lockControl != true)
        {
            InputManager(); // Basic Movement
        }


        CheckIfGrounded(); // Grounded Check
        Slide();
        Crouch();
        WallRun();

        JumpIN(_jumpKey); // Jump Input
    }

    void AttributeReseter()
    {
        _GroundCheckBypass = false;
        _jumpMultiplier = _baseJumpMulitiplier;
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
            _isGrounded = true;
            Debug.DrawLine(this.transform.position, hit.point, Color.red);
            return;
        }

        _isGrounded = false;
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

    private void wallClimb()
    {
        switch (_isGrounded)
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
            switch (_isGrounded)
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
     * -------
     *  Slide
     * -------
     */


    // Function
    // Allows the player to slide
    private void Slide()
    {
        // Returns if player releases the C key
        if (_MoveState != MovementState.Crouch)
        {
            if (Input.GetKeyUp(_crouchKey))
            {
                Debug.Log("Exiting Slide");
                ExitSlide();
                return;
            }
        }

        switch (_isGrounded)
        {
            case true:
                break;
            case false:
                return;
        }

        switch (_MoveState)
        {
            case MovementState.Idle:
            case MovementState.Crouch:
                return;
            default:
                break;
        }

        if (Input.GetKeyDown(_crouchKey))
        {
            _lockControl = true;

            initialForce = _rb.velocity.magnitude;
            initialForce *= 200f;
        }

        if (Input.GetKey(_crouchKey))
        {
            c_CameraRoot.localPosition -= new Vector3(c_CameraRoot.transform.localPosition.x, 1f * Time.deltaTime, c_CameraRoot.transform.localPosition.z);

            if (c_CameraRoot.localPosition.y < 0.3f)
            {
                c_CameraRoot.localPosition = new Vector3(c_CameraRoot.transform.localPosition.x, 0.3f, c_CameraRoot.transform.localPosition.z);
            }

            initialForce /= 2;

            _rb.AddForce(transform.forward * initialForce);

            Mathf.Clamp(_jumpForce, _baseJumpForce, _baseJumpForce * 4);
            _jumpMultiplier += 0.5f;

            if (_rb.velocity.magnitude <= 1f)
            {
                ExitSlide(false);
            }

            _MoveState = MovementState.Sliding;

            return;
        }
    }

    private void ExitSlide(bool resetCamera = true)
    {
        switch (resetCamera)
        {
            case true:
                c_CameraRoot.localPosition = new Vector3(0, 0.6f, 0.2f);
                break;
            default:
                break;
        }
        _lockControl = false;
        // _playerSpeed = tempSpeed;

        _jumpForce = _baseJumpForce;
        initialForce = 0;

        _MoveState = MovementState.Crouch;
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

            switch (_isGrounded)
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

                _GroundCheckBypass = true;
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
                _isWallRunning = false;

                _wallJumpLock = 1;
            }

            // c_CameraRoot.Rotate(Vector3.forward, _cWallRunRotation - _wallRunRotationMaximum, Space.Self);

            // Add force pushing the player up over a set duration
            // Similiar to how the slide works until the player clicks jump
        }
        else
        {
            // _cWallRunRotation -= _WallRunCameraRotationSpeed;

            _UpForceCount = _WallRunLength;

            _wallJumpLock = 1;

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

            _isWallRunning = false;
        }

        c_CameraRoot.Rotate(Vector3.forward, _cWallRunRotation - _wallRunRotationMaximum, Space.Self);
    }

    void WallJump()
    {
        switch (_wallJumpLock != 0)
        {
            case true:
                _jumpForce = _wallRunJumpPower;
                _wallJumpLock -= 1;
                return;
            default:
                break;
        }
    }

    // Function
    // Desc - Detects input to make the player Jump
    // Func Calls - Jump()
    void JumpIN(KeyCode key)
    {
        /*
        switch (_currentlyJumping)
        {
            case false:
                break;

            default:
                return;
        }
        */

        switch (Input.GetKeyDown(key))
        {
            case (true):
                _jumpCount = 1;
                break;
            default:
                break;
        }

        // Checks for other jump types
        // This includes jumps that are not regular e.g. Off the ground.
        switch (Input.GetKeyUp(key))
        {
            case (true):
                _MoveState = MovementState.Idle;
                _jumpIteration = 0;
                break;
            default:
                break;
        }

        // Returns if player has completed the jump
        if (_jumpIteration >= _jumpIterationMaximum)
        {
            _wallJumpLock = 0;
            _MoveState = MovementState.Idle;
            return;
        }

        if (_wallJumpLock == 0)
        {
            return;
        }

        switch (Input.GetKey(key))
        {
            case (true):
                _jumpIteration++;
                _MoveState = MovementState.Jumping;
                JumpOUT(_GroundCheckBypass, _jumpMultiplier);
                break;
            default:
                return;
        }
    }


    private void JumpOUT(bool groundedBypass = false, float jumpMultiplier = 1.0f)
    {
        switch (groundedBypass)
        {
            case true:
                break;
            default:
                switch (_isGrounded)
                {
                    case true:
                        break;
                    default:
                        return;
                }
                break;
       }
        _rb.AddForce(Vector3.up * (_jumpForce) * (jumpMultiplier));
        Debug.Log("Player has attempted to jump");
        // _jumpIteration += 1;
    }
}