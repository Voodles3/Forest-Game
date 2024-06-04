using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    PlayerMovement movementScript;

    void Start()
    {
        movementScript = GetComponentInParent<PlayerMovement>();
    }

    void OnTriggerEnter(Collider other) 
    {
        movementScript.GroundCheck(true);
    }

    void OnTriggerStay(Collider other) 
    {
        movementScript.GroundCheck(true);
    }

    void OnTriggerExit(Collider other) 
    {
        movementScript.GroundCheck(false);
    }
}
