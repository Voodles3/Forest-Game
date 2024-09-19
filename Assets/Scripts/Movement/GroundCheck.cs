using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest.Movement
{
    public class GroundCheck : MonoBehaviour
    {
        PlayerMovement playerMovement;

        void Start()
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }

        void OnTriggerEnter(Collider other) 
        {
            playerMovement.GroundCheck(true);
        }

        void OnTriggerStay(Collider other) 
        {
            playerMovement.GroundCheck(true);
        }

        void OnTriggerExit(Collider other) 
        {
            playerMovement.GroundCheck(false);
        }
    }
}

