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

    public float jumpMultiplier { get; set; } = 1.0f;
    public bool groundedBypass { get; set; }

    private float _jumpForce; // How high the player jumps
    public int jumpCount { private get; set; }
    private int _jumpIteration;

    [SerializeField] private float _baseJumpForce; // Static version of jump force (Used for calculations)
    [SerializeField] private int _jumpIterationMaximum;

    // Start is called before the first frame update
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
        jumpCount = (int)Mathf.Clamp((float)jumpCount, 0, Mathf.Infinity);
    }

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
                jumpCount = 1;
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
                break;
            default:
                break;
        }

        // Returns if player has completed the jump
        if (_jumpIteration >= _jumpIterationMaximum)
        {
            jumpCount -= 1;
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
                JumpOUT(groundedBypass, jumpMultiplier);
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
                switch (_mController.isGrounded)
                {
                    case true:
                        break;
                    default:
                        return;
                }
                break;
        }

        _playerRB.AddForce(Vector3.up * (_jumpForce) * (jumpMultiplier));
        Debug.Log("Player has attempted to jump");
        // _jumpIteration += 1;
    }
}
