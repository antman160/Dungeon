using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Actor : MonoBehaviour
{
    int currentHealth;
    public float speed = 2f;
    public int maxHealth;

    private Transform Player;
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
        Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        agent = GetComponent<NavMeshAgent>();

        if (Player == null || agent == null)
        {
            Debug.LogError("Player or NavMeshAgent not found!");
            return;
        }

        // Set initial agent properties
        agent.speed = speed;
        agent.destination = Player.position;
    }

    void Update()
    {
        if (Player != null && agent != null)
        {
            agent.destination = Player.position;
        }
        else
        {
            Debug.LogError("Player or NavMeshAgent not properly assigned!");
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
}
