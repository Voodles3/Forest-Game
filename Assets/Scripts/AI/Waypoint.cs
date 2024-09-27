using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest.AI
{
    public class Waypoint : MonoBehaviour
    {
        [Tooltip("Waypoint ID must match enemy ID")] [SerializeField] int waypointID;
    }
}

