using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest.Core  
{
    public class CameraHolder : MonoBehaviour
    {
        public Transform cameraPosition;

        // LateUpdate so that physics calculations and other movements are done first, then camera follows
        void LateUpdate()
        {
            transform.position = cameraPosition.position;    
        }
    }
}

