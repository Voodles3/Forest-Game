using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Forest.AI
{
    public class NpcAI : MonoBehaviour
    {
        [Tooltip("NPC ID must match waypoint ID")] [SerializeField] int ID;

        List<Waypoint> waypoints = new();
        System.Random random = new();
        NavMeshAgent agent;
        Waypoint currentWaypoint;
        
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();

            CreateWaypointList();
        }

        void Start()
        {
            FindNextWaypoint();
        }

        void Update()
        {
            UpdateAnimator();
        }
        
        void CreateWaypointList()
        {
            Waypoint[] allWaypoints = FindObjectsByType<Waypoint>(FindObjectsSortMode.None);

            foreach (Waypoint waypoint in allWaypoints)
            {
                if (waypoint.waypointID == ID)
                {
                    waypoints.Add(waypoint);
                }
            }
        }

        void FindNextWaypoint()
        {
            Waypoint nextWaypoint = waypoints[random.Next(waypoints.Count)];

            if (nextWaypoint == currentWaypoint)
            {
                FindNextWaypoint();
                return;
            }
            currentWaypoint = nextWaypoint;
            agent.destination = currentWaypoint.transform.position;
        }

        IEnumerator PauseAtWaypoint()
        {
            yield return new WaitForSeconds(currentWaypoint.GetPauseTime());
            FindNextWaypoint();
        }

        void OnTriggerEnter(Collider other)
        {
            other.TryGetComponent(out Waypoint waypoint);

            if (waypoint == currentWaypoint)
            {
                StartCoroutine(PauseAtWaypoint());
            }
        }

        void UpdateAnimator()
        {
            Vector3 velocity = agent.velocity; // Current speed of NavMesh Agent
            Vector3 localVelocity = transform.InverseTransformDirection(velocity); // Changing velocity from world space to local
            float speed = localVelocity.z / agent.speed;
            GetComponent<Animator>().SetFloat("forwardSpeed", speed); // Setting Blend Tree value based on forward speed
        }

    }
}

