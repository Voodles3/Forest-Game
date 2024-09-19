using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest.Core  
{
    public class CameraHolder : MonoBehaviour
    {
        [SerializeField] Transform cameraPosOnPlayer;
        Vector3 currentVel;

        // LateUpdate so that physics calculations and other movements are done first, then camera follows
        void LateUpdate()
        {
            //transform.position = cameraTransform.position;
            transform.position = Vector3.SmoothDamp(transform.position, cameraPosOnPlayer.position, ref currentVel, 0.02f);
        }
    }
}

