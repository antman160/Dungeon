#if UNITY_EDITOR
using UnityEditor;  // Required for Handles
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Experimental;

public class EnemyNavigation : MonoBehaviour
{
    private EnemyStats stats;
    private EnemyCombat combat;
    private Vector3 _enemyVelocity;
    private bool isGrounded;

    [Header("Detection Settings")]
    public float fov = 90f;  // Field of View in degrees
    public float detectionRange = 10f;  // Detection range
    public float searchPrecision = 3f;  // Distance for considering a blip "checked off"
    public float searchWidth = 8f;  // Search radius during search phase
    public float roamWidth = 50f;  // Max roaming distance
    public float aggroCooldown = 3f;  // Time before switching to search after losing the player
    public float searchDuration = 10f;  // Time spent searching before returning to roam

    public LayerMask detectionMask;  

    private NavMeshAgent agent;
    private Transform player;
    Animator animator;

    private List<GameObject> blips;
    private HashSet<GameObject> checkedBlips = new HashSet<GameObject>();

    private GameObject currentBlip;
    private Vector3 lastKnownPlayerPosition;
    private float lastSeenTime;

    private Vector3 spawnPosition;  

    private enum EnemyState { Roaming, Aggro, Searching }
    private EnemyState currentState = EnemyState.Roaming;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        combat = GetComponent<EnemyCombat>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        agent.speed = stats.speed;
        combat.OnAttack += HandleAttack;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawnPosition = transform.position;
        blips = new List<GameObject>(GameObject.FindGameObjectsWithTag("Blip"));
        PickNextBlip(roamWidth);
    }

    // Enemy states
    void Update()
    {
        //Debug.Log($"Current State: {currentState}");

        isGrounded = agent.isOnNavMesh && agent.isStopped && agent.remainingDistance <= agent.stoppingDistance;
        AvoidNearbyEnemies();

        ApplyGravity();

        switch (currentState)
        {
            case EnemyState.Roaming:
                HandleRoaming();
                break;

            case EnemyState.Aggro:
                HandleAggro();
                break;

            case EnemyState.Searching:
                HandleSearching();
                break;
        }
        SetAnimations();
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            _enemyVelocity.y += stats.gravity * Time.deltaTime;
            agent.Move(_enemyVelocity * Time.deltaTime);
        }
        else if (_enemyVelocity.y < 0)
        {
            _enemyVelocity.y = -2f;
        }
    }

    private void AvoidNearbyEnemies()
    {
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, agent.radius * 2f, detectionMask);

        foreach (var enemy in nearbyEnemies)
        {
            if (enemy.gameObject != gameObject)
            {
                Vector3 pushDir = transform.position - enemy.transform.position;
                agent.Move(pushDir.normalized * Time.deltaTime);
            }
        }
    }


    // PHASE HANDLERS

    //Roaming
    private void HandleRoaming()
    {
        if (PlayerInSight())
        {
            EnterAggroState();
            return;
        }

        if (ReachedDestination())
        {
            CheckOffBlip(currentBlip);
            PickNextBlip(roamWidth);  
        }
    }

    //Aggro
    private void HandleAggro()
    {
        if (PlayerInSight())
        {
            lastSeenTime = Time.time;
            lastKnownPlayerPosition = player.position;
            agent.destination = player.position;
        }
        else if (ReachedDestination() && Time.time - lastSeenTime >= aggroCooldown)
        {
            EnterSearchState();
        }
    }

    //Search
    private void HandleSearching()
    {
        if (PlayerInSight())
        {
            EnterAggroState();
            return;
        }

        if (ReachedDestination())
        {
            CheckOffBlip(currentBlip);
            PickNextBlip(searchWidth);  // reduced search width
        }

        if (Time.time - lastSeenTime >= searchDuration)
        {
            EnterRoamingState();  // Return to roaming after search 
        }
    }


    // STATE SWITCHING
    private void EnterAggroState()
    {
       
        currentState = EnemyState.Aggro;
        Debug.Log($"Current State: {currentState}");
        lastSeenTime = Time.time;
        lastKnownPlayerPosition = player.position;
        agent.destination = lastKnownPlayerPosition;
    }

    private void EnterSearchState()
    {
        
        currentState = EnemyState.Searching;
        Debug.Log($"Current State: {currentState}");
        lastSeenTime = Time.time;
        ResetCheckedBlips();
        PickNextBlip(searchWidth);  // Start searching within reduced width
    }

    private void EnterRoamingState()
    {
       
        currentState = EnemyState.Roaming;
        Debug.Log($"Current State: {currentState}");
        ResetCheckedBlips();
        PickNextBlip(roamWidth);  // Return to full roaming width
    }

    // UTILITY METHODS
    private void PickNextBlip(float maxDistance)
    {
        List<GameObject> availableBlips = new List<GameObject>();

        foreach (var blip in blips)
        {
            float distanceFromSpawn = Vector3.Distance(transform.position, blip.transform.position);
            if (!checkedBlips.Contains(blip) && distanceFromSpawn <= maxDistance)
            {
                availableBlips.Add(blip);
            }
        }

        if (availableBlips.Count > 0)
        {
            currentBlip = availableBlips[Random.Range(0, availableBlips.Count)];
            agent.SetDestination(currentBlip.transform.position);
        }
        else
        {
            currentBlip = FindClosestBlip();  // Fallback if no blips are within range
            agent.SetDestination(currentBlip.transform.position);
        }
    }

    private GameObject FindClosestBlip()
    {
        GameObject closestBlip = null;
        float closestDistance = Mathf.Infinity;

        foreach (var blip in blips)
        {
            float distanceToBlip = Vector3.Distance(transform.position, blip.transform.position);
            if (distanceToBlip < closestDistance)
            {
                closestDistance = distanceToBlip;
                closestBlip = blip;
            }
        }

        return closestBlip;
    }

    private void CheckOffBlip(GameObject blip)
    {
        checkedBlips.Add(blip);
    }

    private void ResetCheckedBlips()
    {
        checkedBlips.Clear();
    }

    private bool ReachedDestination() => Vector3.Distance(transform.position, agent.destination) <= searchPrecision;

    public bool PlayerInSight()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > detectionRange) return false;

        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > fov / 2f) return false;

        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer.normalized, out RaycastHit hit, detectionRange, detectionMask))
        {
            if (hit.transform.CompareTag("Player")) return true;
        }

        return false;
    }

    // ANIMATION
    public const string IDLE = "zombieIdle";
    public const string WALK = "walking";
    public const string ATTACK = "zombieAttack";
    public const string DEATH = "death";
    public const string HIT = "zombieHit";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }
    void HandleAttack()
    {
        ChangeAnimationState(ATTACK);
    }
    void SetAnimations()
    {
        // Only change to movement animations if not attacking
        if (currentAnimationState != ATTACK)
        {
            if (agent.velocity.magnitude == 0)
            {
                ChangeAnimationState(IDLE);
            }
            else
            {
                ChangeAnimationState(WALK);
            }
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        combat.OnAttack -= HandleAttack;
    }

    // DEBUG VISUALIZATION
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Draw Roaming Width (Orange)
        Handles.color = new Color(1f, 0.5f, 0f, 0.5f);  // Orange
        Handles.DrawWireDisc(spawnPosition, Vector3.up, roamWidth);

        // Draw Searching Width (Red)
        Handles.color = new Color(1f, 0f, 0f, 0.5f);  // Red
        Handles.DrawWireDisc(transform.position, Vector3.up, searchWidth);

        // Detection Range (Yellow)
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(transform.position, Vector3.up, detectionRange);

        // Draw FOV Arc and Boundary Lines (Blue)
        Handles.color = Color.blue;

        // Calculate FOV Edge Positions
        Vector3 leftFOVEdge = Quaternion.Euler(0, -fov / 2f, 0) * transform.forward * detectionRange;
        Vector3 rightFOVEdge = Quaternion.Euler(0, fov / 2f, 0) * transform.forward * detectionRange;

        // Draw FOV Arc
        Handles.DrawWireArc(
            transform.position,
            Vector3.up,
            Quaternion.Euler(0, -fov / 2f, 0) * transform.forward,
            fov, detectionRange
        );

        // Draw Boundary Lines from Outer Points to Center
        Handles.DrawLine(transform.position, transform.position + leftFOVEdge);
        Handles.DrawLine(transform.position, transform.position + rightFOVEdge);

        // Indicate Current Blip (Green)
        if (currentBlip != null)
        {
            Handles.color = Color.green;
            Handles.DrawWireDisc(currentBlip.transform.position, Vector3.up, searchPrecision);
        }
    }
#endif
}


