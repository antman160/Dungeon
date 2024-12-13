using UnityEngine;
using System;

public class EnemyCombat : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 4.18f;

    private float lastAttackTime;
    private Transform player;
    private PlayerController playerController;

    // Event triggered when the enemy attacks
    public event Action OnAttack;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();

        }
    }

    void Update()
    {

        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    void Attack()
    {
        if (playerController != null)
        {
            Debug.Log("Enemy attacks the player!");
            playerController.TakeDamage(damage);
            OnAttack?.Invoke();
        }
        else
        {
            Debug.LogWarning("PlayerController not found!");
        }
    }
}


