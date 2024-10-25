using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using Forest.Core;
using Forest.Movement;

namespace Forest.AI
{
    public class MonsterAI : MonoBehaviour
    {
        [SerializeField] State currentState = State.patrolling;
        [SerializeField] bool playerInLOS;
        [SerializeField] bool playerInSight;
        [SerializeField] bool playerInNoiseRadius;

        [Header("General")]
        [Tooltip("Monster ID must match waypoint ID")] [SerializeField] int ID;
        [SerializeField] float viewAngle;
        [SerializeField] float maxLOSRange;
        [SerializeField] float deathRange;

        [Header("Patrolling")]
        [Tooltip("View Distance when Patrolling")] [SerializeField] float patrolRadius;
        [SerializeField] float patrolSpeed;

        [Header("Suspicious")]
        [Tooltip("View Distance when Suspicious")] [SerializeField] float susRadius;
        [Tooltip("Time to stare at sound before investigating")] [SerializeField] float suspiciousStartDelay;
        [SerializeField] float suspiciousSpeed;
        [SerializeField] float suspiciousWaitTime;
        [SerializeField] float noiseCheckInterval = 0.5f;

        [Header("Chasing")]
        [Tooltip("View Distance when Chasing and Searching")] [SerializeField] float chaseRadius;
        [Tooltip("Time to stare at player before chasing")] [SerializeField] float chaseStartDelay;
        [SerializeField] float chaseSpeed;
        

        [Header("Searching")]
        [SerializeField] float searchTime;

        [Header("Debug")]
        [SerializeField] float sightTextOffset;
        
        [Header("References")]
        [SerializeField] TextMeshProUGUI sightText;
        [SerializeField] Transform monsterHead;

        bool arrived;
        float distanceFromPlayer;
        float noiseCheckTimer; 

        List<Waypoint> waypoints = new();
        NavMeshAgent agent;
        Waypoint currentWaypoint;
        DeathHandler deathHandler;
        PlayerMovement playerMovement;
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
            deathHandler = FindObjectOfType<DeathHandler>();
            playerMovement = FindObjectOfType<PlayerMovement>();
            playerCol = deathHandler.GetComponentInChildren<Collider>();

            CreateWaypointList();
        }

        void Start()
        {
            GoToNextWaypoint();
        }

        void Update()
        {
            StateHandler();
            CheckPlayerInLOS();
            CheckPlayerInSight();
            CheckNoiseRadius();
            CheckPlayerDistance();
            UpdateAnimator();
        }

        void StateHandler()
        {
            if (currentState == State.patrolling)
            {
                agent.speed = patrolSpeed;
            }
            else if (currentState == State.suspicious)
            {
                agent.speed = suspiciousSpeed;
            }
            else if (currentState == State.chasing)
            {
                agent.speed = chaseSpeed;
            }
            else if (currentState == State.searching)
            {   
                
            }
        }

        void CheckPlayerInSight()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, GetDetectionRadiusForState());
            bool playerSeen = false;

            foreach (Collider hit in hits)
            {
                if (hit.transform == playerCol.transform)
                {
                    float dotProduct = Vector3.Dot(transform.forward, GetDirectionToPlayer());

                    if (dotProduct > Mathf.Cos(viewAngle * 0.5f * Mathf.Deg2Rad))
                    {
                        playerSeen = true;

                        if (playerInLOS && currentState != State.chasing)
                        {
                            StopAllCoroutines();
                            StartCoroutine(Chase(true));
                            Debug.Log("Start chasing!");
                        }
                        break; // Exit the loop once the player is found
                    }
                }
            }
            playerInSight = playerSeen;
            SightText();
        }

        void CheckPlayerInLOS()
        {
            Ray ray = new(monsterHead.position, GetDirectionToPlayer());
            Physics.Raycast(ray, out RaycastHit hit, maxLOSRange);

            playerInLOS = hit.collider == playerCol; // Player is in LOS if hit.collider is playerCol
        }

        void CheckNoiseRadius()
        {
            noiseCheckTimer += Time.deltaTime;

            if (noiseCheckTimer >= noiseCheckInterval && distanceFromPlayer <= playerMovement.CurrentNoiseRadius && playerMovement.isMoving)
            {
                playerInNoiseRadius = true;
                noiseCheckTimer = 0f;

                if (currentState == State.patrolling)
                {
                    Debug.Log("Heard something, going to investigate...");
                    currentState = State.suspicious;
                    StopAllCoroutines();
                    StartCoroutine(Suspicious(true));
                }
            }
            else
            {
                playerInNoiseRadius = false;
            }
        }

        Vector3 GetDirectionToPlayer()
        {
            return (playerCol.transform.position - monsterHead.position).normalized;
        }

        float GetDetectionRadiusForState()
        {
            return currentState switch
            {
                State.patrolling => patrolRadius,
                State.suspicious => susRadius,
                State.chasing or State.searching => chaseRadius,
                _ => patrolRadius,
            };
        }

        void SightText()
        {
            if (playerInSight && playerInLOS)
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
            // Get the dynamic detection radius based on the current state
            float radius = GetDetectionRadiusForState();

            // Set the color and draw the detection radius sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);

            // Draw the FOV cone
            Gizmos.color = Color.red;
            Vector3 forwardDirection = transform.forward * radius; // Extend based on radius
            Quaternion leftRayRotation = Quaternion.AngleAxis(-viewAngle * 0.5f, Vector3.up);
            Quaternion rightRayRotation = Quaternion.AngleAxis(viewAngle * 0.5f, Vector3.up);

            Vector3 leftRayDirection = leftRayRotation * forwardDirection;
            Vector3 rightRayDirection = rightRayRotation * forwardDirection;

            // Draw both sides of the FOV cone
            Gizmos.DrawRay(transform.position, leftRayDirection);
            Gizmos.DrawRay(transform.position, rightRayDirection);
        }

        IEnumerator Suspicious(bool delay)
        {
            agent.ResetPath();
            Vector3 noisePosition = playerCol.transform.position;

            if (delay)
            {
                float endTime = Time.time + suspiciousStartDelay;
                while (Time.time < endTime)
                {
                    Vector3 direction = (playerCol.transform.position - transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
                    yield return null;
                }
            }
            agent.destination = noisePosition;

            while (playerInNoiseRadius)
            {
                agent.destination = playerCol.transform.position;
                yield return new WaitForSeconds(0.1f);
            }

            while (agent.pathPending || (agent.hasPath && agent.remainingDistance > agent.stoppingDistance))
            {
                if (playerInNoiseRadius)
                {
                    StartCoroutine(Suspicious(false));
                    yield break;
                }
                yield return null;
            }
            yield return new WaitForSeconds(suspiciousWaitTime);

            Debug.Log("Gave up, going back to patrolling");
            currentState = State.patrolling;
            GoToClosestWaypoint();
        }

        IEnumerator Chase(bool delay)
        {
            currentState = State.chasing;

            if (delay)
            {
                agent.ResetPath();
                float endTime = Time.time + chaseStartDelay;
                while (Time.time < endTime)
                {
                    Vector3 direction = (playerCol.transform.position - transform.position).normalized;
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 20f * Time.deltaTime);
                    yield return null;
                }
            }
            
            agent.destination = playerCol.transform.position;

            while (playerInLOS)
            {
                agent.destination = playerCol.transform.position;
                yield return new WaitForSeconds(0.1f);
            }

            // Player has gone out of LOS, but monster hasn't reached their last known position yet.

            while (agent.pathPending || (agent.hasPath && agent.remainingDistance > agent.stoppingDistance))
            {
                if (playerInLOS) // If player gets in LOS again while he's traveling to last known position, he starts chasing again
                {
                    StartCoroutine(Chase(false));
                    yield break;
                }
                yield return null;
            }
            // Player went out of LOS, and he has reached their last known position
            //Debug.Log("Reached player's last known position, starting to search");

            StartCoroutine(Search());
        }

        IEnumerator Search()
        {
            currentState = State.searching;

            float endTime = Time.time + searchTime;
            while (Time.time < endTime)
            {
                if (playerInNoiseRadius)
                {
                    StartCoroutine(Chase(false));
                    yield break;
                }
                yield return null;
            }
            // TODO: Make monster look around while he searches

            Debug.Log("Gave up, going back to patrolling");
            currentState = State.patrolling;
            GoToClosestWaypoint();
        }

        void CheckPlayerDistance()
        {
            distanceFromPlayer = Vector3.Distance(transform.position, playerCol.transform.position);

            if (distanceFromPlayer <= deathRange)
            {
                currentState = State.patrolling;
                StopAllCoroutines();
                GoToClosestWaypoint();

                KillPlayer();
            }
        }

        void KillPlayer()
        {
            deathHandler.Die();
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

            if (nextWaypoint != currentWaypoint)
            {
                currentWaypoint = nextWaypoint;
                agent.destination = currentWaypoint.transform.position;
                arrived = false;
            }
            else
            {
                GoToNextWaypoint();
            }
            
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

        void OnTriggerEnter(Collider other)
        {
            //float distance = Vector3.Distance(transform.position, currentWaypoint.transform.position);
            other.TryGetComponent(out Waypoint waypoint);

            if (waypoint == currentWaypoint && currentState == State.patrolling && !arrived)
            {
                arrived = true;
                StartCoroutine(PauseAtWaypoint());
            }
        }

        IEnumerator PauseAtWaypoint()
        {
            yield return new WaitForSeconds(currentWaypoint.GetPauseTime());
            GoToNextWaypoint();
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

