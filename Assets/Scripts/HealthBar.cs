using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider healthBar;
    private PlayerController playerController;

    private void Start()
    {
        // Find the Player's PlayerController component and assign it
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        healthBar = GetComponent<Slider>();

        // Initialize Health Bar
        healthBar.maxValue = playerController.maxHealth;
        healthBar.value = playerController.maxHealth;
    }

    private void Update()
    {
        // Update health bar dynamically based on player's current health
        healthBar.value = playerController.currentHealth;
    }
}

