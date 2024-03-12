using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerJump))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MovementController))]
public class PlayerWallRun : MonoBehaviour
{
    [SerializeField] Transform c_CameraRoot;

    private MovementController _mController;
    private Rigidbody _playerRB;

    private PlayerJump c_Jump;

    private int _UpForceCount;

    [SerializeField] private int _WallRunLength = 100;
    [SerializeField] private float _wallRunRotationMaximum = 10.0f;
    [SerializeField] private float _cWallRunRotation;
    [SerializeField] private float _WallRunCameraRotationSpeed = 0.5f;
    [SerializeField] private float _wallRunJumpPower = 1.5f;

    bool enteredWallJump = false;

    // Start is called before the first frame update
    void Start()
    {
        _mController = GetComponent<MovementController>();
        _playerRB = GetComponent<Rigidbody>();

        c_Jump = GetComponent<PlayerJump>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void WallRun()
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

            switch (_mController.isGrounded)
            {
                case false:
                    break;
                default:
                    return;
            }
            if(enteredWallJump == false)
            {
                c_Jump.jumpCount += 1;
                enteredWallJump = true;
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
                    _playerRB.AddForce(transform.up * 1);
                    _playerRB.AddForce(transform.forward * 5);
                    _UpForceCount--;
                }

                c_Jump.jumpMultiplier = 1.5f;
                // c_Jump.groundedBypass = true;
                _mController.SetMovestate(MovementState.WallRunning);
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

                enteredWallJump = false;
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

            enteredWallJump = false;
        }

        c_CameraRoot.Rotate(Vector3.forward, _cWallRunRotation - _wallRunRotationMaximum, Space.Self);
    }
}
