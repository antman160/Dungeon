using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Actor : MonoBehaviour
{
    int currentHealth;
    public float speed = 2f;
    public int maxHealth;

    public float fov = 90f; // Field of View in degrees
    public float detectionRange = 10f; // Detection range
    public LayerMask detectionMask; // Layer mask for detection (e.g., Player layer)

    public bool showDebug = true; // Toggle for debugging visuals

    private Transform player;
    private NavMeshAgent agent;

    // Define the OnDestroyed event
    public delegate void ActorDestroyed(GameObject actor);
    public event ActorDestroyed OnDestroyed;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        // Find the player and NavMeshAgent
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();

        if (player == null || agent == null)
        {
            Debug.LogError("Player or NavMeshAgent not found!");
            return;
        }

        // Set initial agent properties
        agent.speed = speed;
    }

    void Update()
    {
        if (PlayerInSight())
        {
            // Follow the player if they are detected
            agent.destination = player.position;
        }
        else
        {
            // Stop moving if the player is not detected
            agent.ResetPath();
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        Debug.Log("AI has died.");
        Destroy(gameObject); // Trigger OnDestroy
    }

    void OnDestroy()
    {
        // Trigger the OnDestroyed event when this object is destroyed
        OnDestroyed?.Invoke(gameObject);
    }

    bool PlayerInSight()
    {
        if (player == null) return false;

        // Calculate the direction to the player
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        // Check if the player is within range
        if (distanceToPlayer > detectionRange) return false;

        // Check if the player is within the FOV
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > fov / 2f) return false;

        // Perform a raycast to check for line of sight
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, directionToPlayer.normalized, out hit, detectionRange, detectionMask))
        {
            if (hit.transform.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    void OnDrawGizmos()
    {
        // Draw debugging visuals if enabled
        if (showDebug)
        {
            // Visualize detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // Visualize FOV cone
            Gizmos.color = Color.blue;
            Vector3 forward = transform.forward;
            Quaternion leftRayRotation = Quaternion.Euler(0, -fov / 2f, 0);
            Quaternion rightRayRotation = Quaternion.Euler(0, fov / 2f, 0);

            Vector3 leftRayDirection = leftRayRotation * forward * detectionRange;
            Vector3 rightRayDirection = rightRayRotation * forward * detectionRange;

            Gizmos.DrawRay(transform.position, leftRayDirection);
            Gizmos.DrawRay(transform.position, rightRayDirection);
        }
    }
}

