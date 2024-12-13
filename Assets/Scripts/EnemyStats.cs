using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float speed = 2f;
    public float gravity = -9.8f;

    public delegate void EnemyDeath(GameObject enemy);
    public event EnemyDeath OnDeath;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        // Log when the enemy is hit
        Debug.Log($"{gameObject.name} hit! Current Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died.");
        OnDeath?.Invoke(gameObject); // Notify other systems of death
        Destroy(gameObject);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}
