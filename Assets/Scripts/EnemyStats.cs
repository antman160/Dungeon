using UnityEngine;
using UnityEngine.AI;

public class EnemyStats : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    public float speed = 2f;
    public float gravity = -9.8f;

    public delegate void EnemyDeath(GameObject enemy);
    public event EnemyDeath OnDeath;

    private Animator animator;
    private NavMeshAgent agent;
    

    private static readonly string DEATH_ANIMATION = "death";  // Animation trigger name
    private float deathAnimationDuration = 2.9f;  // Duration of the death animation

    void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
       
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        Debug.Log($"{gameObject.name} hit! Current Health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemy has died.");

        // Play death animation
        if (animator != null)
        {
            animator.CrossFadeInFixedTime(DEATH_ANIMATION, 0.2f);
        }

        // Stop motion
        StopMotion();

        OnDeath?.Invoke(gameObject);  // Notify other systems of death

        // Start coroutine to wait and destroy
        StartCoroutine(WaitAndDestroy());
    }

    private void StopMotion()
    {
        if (agent != null)
        {
            agent.isStopped = true;        
            //agent.enabled = false;         
        }

       
    }

    private System.Collections.IEnumerator WaitAndDestroy()
    {
        // Wait for animation to complete
        yield return new WaitForSeconds(deathAnimationDuration); 
        Destroy(gameObject); 
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}


