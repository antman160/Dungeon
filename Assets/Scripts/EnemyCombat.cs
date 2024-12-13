using UnityEngine;

public class EnemyCombat : MonoBehaviour
{
    public int damage = 10;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;

    private float lastAttackTime;
    private Transform player;
    private PlayerController playerController;
    private EnemyNavigation enemyNavigation;

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerController = playerObject.GetComponent<PlayerController>();
        }

        enemyNavigation = GetComponent<EnemyNavigation>();
        if (enemyNavigation == null)
        {
            Debug.LogWarning("EnemyNavigation component not found!");
        }
    }

    void Update()
    {
        if (player != null && enemyNavigation != null && enemyNavigation.PlayerInSight())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && Time.time - lastAttackTime >= attackCooldown)
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
            //playerController.TakeDamage(damage);
        }
        else
        {
            Debug.LogWarning("PlayerController not found!");
        }
    }
}

