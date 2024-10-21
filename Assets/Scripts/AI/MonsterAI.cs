using System.Collections;
using System.Collections.Generic;
using Forest.Movement;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace Forest.AI
{
    public class MonsterAI : MonoBehaviour
    {
        [SerializeField] State currentState = State.patrolling;
        [SerializeField] bool playerInLOS;
        [SerializeField] bool playerInSightBox;

        [Header("General")]
        [Tooltip("Monster ID must match waypoint ID")] [SerializeField] int ID;
        [SerializeField] float maxLOSRange;

        [Header("Patrolling")]
        [Tooltip("View Distance when Patrolling")] [SerializeField] float patrolVD;
        [SerializeField] float patrolSpeed;

        [Header("Suspicious")]
        [Tooltip("View Distance when Suspicious")] [SerializeField] float suspiciousVD;
        [SerializeField] float suspiciousSpeed;

        [Header("Chasing")]
        [Tooltip("View Distance when Chasing and Searching")] [SerializeField] float chaseVD;
        [SerializeField] float chaseSpeed;
        [Tooltip("Time to stare at player before chasing")][SerializeField] float chaseStartDelay;

        [Header("Searching")]
        [SerializeField] float searchTime;

        [Header("Debug")]
        [SerializeField] float sightTextOffset;
        
        [Header("References")]
        [SerializeField] TextMeshProUGUI sightText;
        [SerializeField] Transform monsterHead;

        Vector3 sightBox;
        float sightBoxOffset = 3.5f;

        List<Waypoint> waypoints = new();
        NavMeshAgent agent;
        Waypoint currentWaypoint;
        Collider playerCol;

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
            playerCol = FindObjectOfType<PlayerMovement>().GetComponentInChildren<Collider>();

            CreateWaypointList();
        }

        void Start()
        {
            sightBox = new(20, 5, patrolVD);
            sightBoxOffset = transform.localScale.z - 0.5f + sightBox.z / 2;
            GoToNextWaypoint();
        }

        void Update()
        {
            StateHandler();
            CheckPlayerInLOS();
            CheckPlayerInSightBox();
            UpdateAnimator();
        }

        void StateHandler()
        {
            if (currentState == State.patrolling)
            {
                sightBox = new(patrolVD * 2, 5, patrolVD);
                agent.speed = patrolSpeed;
            }
            else if (currentState == State.suspicious)
            {
                sightBox = new(suspiciousVD * 2, 5, suspiciousVD);
                agent.speed = suspiciousSpeed;
            }
            else if (currentState == State.chasing)
            {
                sightBox = new(chaseVD * 2, 5, chaseVD);
                agent.speed = chaseSpeed;
            }
            else if (currentState == State.searching)
            {   
                sightBox = new(chaseVD * 2, 5, chaseVD);
            }
        }

        void CheckPlayerInSightBox()
        {
            Vector3 boxCenter = transform.position + transform.forward * sightBoxOffset;
            Collider[] hits = Physics.OverlapBox(boxCenter, sightBox / 2, transform.rotation);
            bool playerFound = false;

            foreach (Collider hit in hits)
            {
                if (hit.transform == playerCol.transform)
                {
                    playerFound = true;

                    if (playerInLOS && currentState != State.chasing)
                    {
                        StopAllCoroutines();
                        StartCoroutine(Chase());
                        Debug.Log("Start chasing!");
                    }
                    break; // Exit the loop once the player is found
                }
            }
            playerInSightBox = playerFound;
            SightText();
        }

        void CheckPlayerInLOS()
        {
            Vector3 directionToPlayer = (playerCol.transform.position - monsterHead.position).normalized;
            Ray ray = new(monsterHead.position, directionToPlayer);
            Physics.Raycast(ray, out RaycastHit hit, maxLOSRange);

            playerInLOS = hit.collider == playerCol; // Player is in LOS if hit.collider is playerCol
        }

        void SightText()
        {
            if (playerInSightBox && playerInLOS)
            {
                sightText.text = "Player in sight";
                sightText.color = Color.green;
            }
            else
            {
                sightText.text = "Player not in sight";
                sightText.color = Color.red;
            }
            sightText.transform.SetPositionAndRotation(transform.position + Vector3.up * sightTextOffset, Camera.main.transform.rotation);
        }

        void OnDrawGizmosSelected()
        {
            sightBoxOffset = transform.localScale.z - 0.5f + 5;
            Vector3 boxCenter = transform.position + transform.forward * sightBoxOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(boxCenter, new(20, 5, 10));
        }

        IEnumerator Chase()
        {
            currentState = State.chasing;
            agent.ResetPath();

            float endTime = Time.time + chaseStartDelay;
            while (Time.time < endTime)
            {
                Vector3 direction = (playerCol.transform.position - transform.position).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(direction);

                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);
                yield return null;
            }
            yield return new WaitForSeconds(chaseStartDelay);

            while (playerInLOS)
            {
                agent.destination = playerCol.transform.position;
                yield return new WaitForSeconds(0.1f);
            }
            // Player has gone out of LOS, but monster hasn't reached their last known position yet.
            while (agent.hasPath)
            {
                if (playerInLOS) // If player gets in LOS again while he's traveling to last known position, he starts chasing again
                {
                    StartCoroutine(Chase());
                    yield break;
                }
                yield return null;
            }
            // Player went out of LOS, and he has reached their last known position
            Debug.Log("Reached player's last known position, starting to search");

            StartCoroutine(Search());
        }

        IEnumerator Search()
        {
            currentState = State.searching;
            yield return new WaitForSeconds(searchTime); // TODO: Make monster look around while he waits here

            Debug.Log("Gave up, going back to patrolling");
            currentState = State.patrolling;
            GoToClosestWaypoint();
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

        void GoToNextWaypoint()
        {
            Waypoint nextWaypoint = waypoints[random.Next(waypoints.Count)];

            if (nextWaypoint == currentWaypoint)
            {
                GoToNextWaypoint();
                return;
            }
            currentWaypoint = nextWaypoint;
            agent.destination = currentWaypoint.transform.position;
        }

        void GoToClosestWaypoint()
        {
            float closestDistance = Mathf.Infinity;
            foreach (Waypoint waypoint in waypoints)
            {
                float distance = Vector3.Distance(transform.position, waypoint.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    currentWaypoint = waypoint;
                }
            }
            agent.destination = currentWaypoint.transform.position;
        }

        IEnumerator PauseAtWaypoint()
        {
            yield return new WaitForSeconds(currentWaypoint.GetPauseTime());
            GoToNextWaypoint();
        }

        void OnTriggerEnter(Collider other)
        {
            other.TryGetComponent(out Waypoint waypoint);

            if (waypoint == currentWaypoint && currentState == State.patrolling)
            {
                StartCoroutine(PauseAtWaypoint());
            }
        }
        #endregion

        void UpdateAnimator()
        {
            Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity); // Changing velocity from world space to local
            float speed = localVelocity.z / chaseSpeed;
            GetComponent<Animator>().SetFloat("forwardSpeed", speed); // Setting Blend Tree value based on forward speed
        }

    }
}

