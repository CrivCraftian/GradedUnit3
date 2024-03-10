using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public GameObject playerObject;

    public float _mSensitivity = 5;

    float xRotation = 0;

    private Vector3 StartPosition;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        StartPosition = transform.position;
    }

    // Update is called once per frame7

    private void Update()
    {
        CameraToMouse();
        // CameraOnObject();
    }

    void CameraOnObject()
    {
        this.transform.position = playerObject.transform.position + StartPosition;
    }

    void CameraToMouse()
    {
        float mInputHorizontal = Input.GetAxis("Mouse X") * _mSensitivity * Time.deltaTime;
        float mInputVertical = Input.GetAxis("Mouse Y") * _mSensitivity * Time.deltaTime;

        xRotation -= mInputVertical;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        playerObject.transform.Rotate(Vector3.up * mInputHorizontal);
    }

    void tmpMouseLock()
    {
        if(Input.GetKeyDown(KeyCode.Keypad1))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        if(Input.GetKeyDown(KeyCode.Keypad2))
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
