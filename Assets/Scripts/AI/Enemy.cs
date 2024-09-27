using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Forest.AI
{
    public class Enemy : MonoBehaviour
    {
        [Tooltip("Enemy ID must match waypoint ID")] [SerializeField] int enemyID;
        [SerializeField] List<Waypoint> waypoints = new();

    }
}

