using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
using UnityEngine.Device;

public class Robot : MonoBehaviour
{
    Rigidbody _rb;

    [SerializeField] GameObject _player;

    float m_movementSpeed;
    float m_rotationSpeed;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {
        RotateTowardsPlayer(_player, m_rotationSpeed);
    }

    private void RotateTowardsPlayer(GameObject target, float rotationSpeed)
    {
        /*
        float angleRadians = Mathf.Atan2(target.transform.position.z - this.transform.position.z, target.transform.position.x - this.transform.position.x);
        float angleDegrees = (float)((180 / Mathf.PI) * angleRadians);

        
        if(angleDegrees < 0)
        {
            angleDegrees = 180 - angleDegrees;
        }
        

        angleDegrees = 360 - angleDegrees;

        transform.rotation = Quaternion.Euler(transform.rotation.x, angleDegrees, transform.rotation.z);
        */

        transform.LookAt(target.transform);

        /*
        transform.Rotate(Vector3.up, 1);
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
        */
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null)
            return;
        /*
        if(collision.collider.tag == "Player")
        {
            collision.gameObject.transform.parent = transform;
        }
        */
    }
}
