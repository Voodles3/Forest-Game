using System.Collections;
using System.Collections.Generic;
using System.Data;
using Forest.Movement;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Forest.AI
{
    public class MonsterAI : MonoBehaviour
    {
        [Tooltip("Monster ID must match waypoint ID")] [SerializeField] int ID;
        [SerializeField] State currentState = State.patrolling;
        
        [Tooltip("View Distance when Patrolling")][SerializeField] float patrolVD;
        [Tooltip("View Distance when Suspicious")][SerializeField] float suspiciousVD;
        [Tooltip("View Distance when Chasing and Searching")][SerializeField] float chaseVD;
        float viewDistance;
        Vector3 boxSize;
        float boxDistance = 3.5f;

        List<Waypoint> waypoints = new();
        NavMeshAgent agent;
        Waypoint currentWaypoint;
        Transform playerTransform;
        [SerializeField] TextMeshProUGUI sightText;

        readonly System.Random random = new();
        
        enum State
        {
            patrolling,
            suspicious,
            chasing,
            searching
        }
        
        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            playerTransform = FindObjectOfType<PlayerMovement>().GetComponentInChildren<Collider>().transform;

            CreateWaypointList();
        }

        void Start()
        {
            boxSize = new(15, 5, patrolVD);
            boxDistance = transform.localScale.z - 0.5f + boxSize.z / 2;
            FindNextWaypoint();
        }

        void Update()
        {
            StateHandler();
            CheckSightRange();
        }

        void StateHandler()
        {
            if (currentState == State.patrolling)
            {
                viewDistance = patrolVD;
                boxSize.z = patrolVD;
            }
            else if (currentState == State.suspicious)
            {
                viewDistance = suspiciousVD;
                boxSize.z = suspiciousVD;
            }
            else if (currentState == State.chasing)
            {
                viewDistance = chaseVD;
                boxSize.z = chaseVD;
            }
            else if (currentState == State.searching)
            {   
                viewDistance = chaseVD;
                boxSize.z = chaseVD;
            }
        }

        void CheckSightRange()
        {
            Vector3 boxCenter = transform.position + transform.forward * boxDistance;

            Collider[] hits = Physics.OverlapBox(boxCenter, boxSize / 2, transform.rotation);
            bool playerInSight = false;
            foreach (Collider hit in hits)
            {
                if (hit.transform == playerTransform)
                {
                    Debug.Log("Player is in sight!");
                    playerInSight = true;
                }
            }
            if (playerInSight)
            {
                sightText.text = "Player in sight";
                sightText.color = Color.green;
            }
            else
            {
                sightText.text = "Player not in sight";
                sightText.color = Color.red;
            }
            sightText.transform.SetPositionAndRotation(transform.position + Vector3.up * 2, Camera.main.transform.rotation);
        }

        void OnDrawGizmosSelected()
        {
            boxDistance = transform.localScale.z - 0.5f + 5;
            Vector3 boxCenter = transform.position + transform.forward * boxDistance;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, new(15, 5, patrolVD));
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
            yield return new WaitForSeconds(currentWaypoint.GetStopTime());
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
    }
}

