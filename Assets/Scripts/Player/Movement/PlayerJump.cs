using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MovementController))]
public class PlayerJump : MonoBehaviour
{
    private MovementController _mController;
    private Rigidbody _playerRB;

    private KeyCode _jumpKey = KeyCode.Space;

    public float jumpMultiplier { get; set; } = 1.0f; // Jump Multiplier, allows other classes to increase jump power
    public bool groundedBypass { get; set; } // Bypass to allow other classes to override the grounded check

    private float _jumpForce; // How high the player jumps
    private int _baseJumpCount = 2;
    [SerializeField] public int jumpCount = 1;
    private int _jumpIteration; // Used to count how much force has been applied in the jump

    [SerializeField] private float _baseJumpForce; // The base value for jump force
    [SerializeField] private int _jumpIterationMaximum; // Maximum Value for jump iterations

    bool leftGround = false;

    // Gets various components needed to Jump
    void Start()
    {
        _mController = GetComponent<MovementController>();
        _playerRB = GetComponent<Rigidbody>();

        _jumpForce = _baseJumpForce;
    }

    private void LateUpdate()
    {
        groundedBypass = false;
        jumpMultiplier = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        // Clamps Jump Count to keep it from going below 0
        jumpCount = (int)Mathf.Clamp((float)jumpCount, 0, Mathf.Infinity);
    }

    // Function
    // Desc - Sets keybinding for Jump to a new Key
    public void SetKey(KeyCode key)
    {
        _jumpKey = key;
    }

    // Function
    // Desc - Detects input to make the player Jump
    // Func Calls - Jump()
    public void JumpIN()
    {
        switch (Input.GetKeyDown(_jumpKey))
        {
            case (true):
                jumpCount -= 1;
                break;
            default:
                break;
        }

        // Checks for other jump types
        // This includes jumps that are not regular e.g. Off the ground.
        switch (Input.GetKeyUp(_jumpKey))
        {
            case (true):
                _jumpIteration = 0;

                leftGround = true;

                break;
            default:
                break;
        }

        if (_mController.isGrounded == true && leftGround == true || groundedBypass == true && leftGround == true)
        {
            jumpCount = _baseJumpCount;
            leftGround = false;
        }

        if (_jumpIteration >= _jumpIterationMaximum)
        {
            return;
        }

        if (jumpCount == 0)
        {
            return;
        }

        switch (Input.GetKey(_jumpKey))
        {
            case (true):
                _jumpIteration++;
                _mController.SetMovestate(MovementState.Jumping);
                JumpOUT();
                break;
            default:
                return;
        }
    }

    // Function
    // Desc - Applies upwards force to the player
    private void JumpOUT()
    {
        _playerRB.AddForce(Vector3.up * (_jumpForce) * (jumpMultiplier));

        Debug.Log("Player has attempted to jump");
    }
}
