using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MovementController))]
public class PlayerSlide : MonoBehaviour
{
    [SerializeField] Transform c_CameraRoot;

    private MovementController _mController;
    private Rigidbody _playerRB;

    private PlayerJump c_Jump;

    private KeyCode _slideKey = KeyCode.LeftControl;

    private float _slideForce = 0;

    [SerializeField] private float _slideMaximum;

    // Start is called before the first frame update
    void Start()
    {
        // c_CameraRoot = GetComponentInChildren<Transform>();

        _mController = GetComponent<MovementController>();
        _playerRB = GetComponent<Rigidbody>();

        c_Jump = GetComponent<PlayerJump>();
    }

    // Update is called once per frame
    void Update()
    {
            
    }

    public void SetKey(KeyCode sKey)
    {
        _slideKey = sKey;
    }

    public void Slide()
    {
        // Returns if player releases the C key
        if (_mController.CurrentState() != MovementState.Crouch)
        {
            if (Input.GetKeyUp(_slideKey))
            {
                Debug.Log("Exiting Slide");
                ExitSlide();
                return;
            }
        }

        switch (_mController.isGrounded)
        {
            case true:
                break;
            case false:
                return;
        }

        switch (_mController.CurrentState())
        {
            case MovementState.Idle:
            case MovementState.Crouch:
                return;
            default:
                break;
        }

        if (Input.GetKeyDown(_slideKey))
        {
            _mController.ControlsLocked = true;

            _slideForce = _playerRB.velocity.magnitude;
            _slideForce *= _slideMaximum;
        }

        if (Input.GetKey(_slideKey))
        {
            c_CameraRoot.localPosition -= new Vector3(c_CameraRoot.transform.localPosition.x, 1f * Time.deltaTime, c_CameraRoot.transform.localPosition.z);

            if (c_CameraRoot.localPosition.y < 0.3f)
            {
                c_CameraRoot.localPosition = new Vector3(c_CameraRoot.transform.localPosition.x, 0.3f, c_CameraRoot.transform.localPosition.z);
            }

            _slideForce /= 2;

            _playerRB.AddForce(transform.forward * _slideForce);

            c_Jump.jumpMultiplier *= 1.5f;

            if (_playerRB.velocity.magnitude <= 1f)
            {
                ExitSlide(false);
            }

            _mController.SetMovestate(MovementState.Sliding);

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
        _mController.ControlsLocked = false;

        _slideForce = 0f;

        _mController.SetMovestate(MovementState.Crouch);
    }
}
