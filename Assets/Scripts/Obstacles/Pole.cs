using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    [SerializeField] int rotationAngle = 90;
    [SerializeField] float rotationSpeed;

    private bool playerCollided;
    private int counter;

    Transform hitObject;

    // Start is called before the first frame update
    void Start()
    {
        counter = 0;
        playerCollided = false;
    }

    // Update is called once per frame
    void Update()
    {
        RotatePlayer();
    }

    void RotatePlayer()
    {
        switch(playerCollided)
        {
            case true:
                break;
            default:
                return;
        }

        if(counter == 0)
        {
            if(hitObject == null)
            {
                return;
            }
            hitObject.parent = null;
            hitObject.GetComponent<Rigidbody>().AddForce(hitObject.transform.forward*10);

            return;
        }

        this.transform.Rotate(new Vector3(0, -1, 0));

        counter--;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            Debug.Log("Player has collided");
            other.transform.parent = this.transform;

            counter = rotationAngle;
            hitObject = other.transform;
            playerCollided = true;
        }
    }
}
