using UnityEngine;

public class EnemyEvents : MonoBehaviour
{
    private EnemyStats stats;

    void Start()
    {
        stats = GetComponent<EnemyStats>();
        if (stats != null)
        {
            stats.OnDeath += HandleDeath;
        }
    }

    private void HandleDeath(GameObject enemy)
    {
        Debug.Log($"Enemy {enemy.name} has died.");
        // Add additional logic for when an enemy dies, such as spawning loot or notifying a spawner
    }

    void OnDestroy()
    {
        if (stats != null)
        {
            stats.OnDeath -= HandleDeath;
        }
    }
}
