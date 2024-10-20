using System.Collections;
using System.Collections.Generic;
using Forest.Movement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace Forest.AI
{
    public class MonsterAI : MonoBehaviour
    {
        [Tooltip("Monster ID must match waypoint ID")] [SerializeField] int ID;
        [SerializeField] State currentState = State.patrolling;
        
        [Tooltip("View Distance when Patrolling")] [SerializeField] float patrolVD;
        [Tooltip("View Distance when Suspicious")] [SerializeField] float suspiciousVD;
        [Tooltip("View Distance when Chasing and Searching")] [SerializeField] float chaseVD;

        float viewDistance;
        Vector3 sightBox;
        float sightBoxOffset = 3.5f;

        [SerializeField] TextMeshProUGUI sightText;
        [SerializeField] Transform monsterHead;

        List<Waypoint> waypoints = new();
        NavMeshAgent agent;
        Waypoint currentWaypoint;
        Transform playerTransform;

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
            sightBox = new(15, 5, patrolVD);
            sightBoxOffset = transform.localScale.z - 0.5f + sightBox.z / 2;
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
                sightBox.z = patrolVD;
            }
            else if (currentState == State.suspicious)
            {
                sightBox.z = suspiciousVD;
            }
            else if (currentState == State.chasing)
            {
                sightBox.z = chaseVD;
            }
            else if (currentState == State.searching)
            {   
                sightBox.z = chaseVD;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            other.TryGetComponent(out Waypoint waypoint);

            if (waypoint == currentWaypoint)
            {
                StartCoroutine(PauseAtWaypoint());
            }
        }

        void CheckSightRange()
        {
            Vector3 boxCenter = transform.position + transform.forward * sightBoxOffset;

            Collider[] hits = Physics.OverlapBox(boxCenter, sightBox / 2, transform.rotation);
            bool playerInSight = false;
            foreach (Collider hit in hits)
            {
                if (hit.transform == playerTransform)
                {
                    StartChase();
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

        bool GetPlayerInLOS()
        {
            Physics.Raycast(monsterHead.position, transform.forward, out RaycastHit hit);
            if (hit.transform == playerTransform) { return true; }
            return false;
        }

        void OnDrawGizmosSelected()
        {
            sightBoxOffset = transform.localScale.z - 0.5f + 5;
            Vector3 boxCenter = transform.position + transform.forward * sightBoxOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, new(15, 5, patrolVD));
        }
        
        #region Patrolling
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
        #endregion

        void StartChase()
        {

        }
        IEnumerator Chase()
        {
            yield return null;
            currentState = State.chasing;
            sightBox.z = 100f;


        }
    }
}

