using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(MovementController))]
public class PlayerCrouch : MonoBehaviour
{
    [SerializeField] Transform c_CameraRoot;

    private MovementController _mController;
    private Rigidbody _playerRB;

    private KeyCode crouchKey = KeyCode.LeftControl;

    // Start is called before the first frame update
    void Start()
    {
        _mController = GetComponent<MovementController>();
        _playerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetKey(KeyCode cKey)
    {
        crouchKey = cKey;
    }

    public void Crouch()
    {
        // Returns if player is not grounded
        if (_mController.CurrentState() != MovementState.Crouch)
        {
            switch (_mController.isGrounded)
            {
                case true:
                    break;
                default:
                    return;
            }

            // Returns if the player is currently sliding
            switch (_mController.CurrentState())
            {
                case MovementState.Idle:
                    break;
                default:
                    return;
            }
        }

        if (Input.GetKey(crouchKey))
        {
            _mController.SetMovestate(MovementState.Crouch);
            c_CameraRoot.localPosition = new Vector3(c_CameraRoot.transform.localPosition.x, 0.3f, c_CameraRoot.transform.localPosition.z);
        }

        // If the player releases C, heighten Camera
        if (Input.GetKeyUp(crouchKey))
        {
            c_CameraRoot.localPosition = new Vector3(c_CameraRoot.transform.localPosition.x, 0.6f, c_CameraRoot.transform.localPosition.z);

            _mController.SetMovestate(MovementState.Idle);
        }
    }
}
