using UnityEngine;

namespace Forest.AI
{
    public class Waypoint : MonoBehaviour
    {
        [Tooltip("Waypoint ID must match Agent ID")] public int waypointID;
        public float timeToSpend;
        public bool randomizeTime;
        public float minRandomTime;
        public float maxRandomTime;

        public float GetPauseTime()
        {
            if (!randomizeTime) { return timeToSpend; }

            return Random.Range(minRandomTime, maxRandomTime);
        }
    }
}

