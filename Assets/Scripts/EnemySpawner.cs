using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab of the enemy to spawn
    public List<Transform> spawnPoints; // List of spawn points
    public float waveInterval = 10f; // Time between waves (in seconds)
    public int enemiesPerWave = 3; // Default number of enemies per wave
    public int maxEnemies = 10; // Maximum number of active enemies allowed

    private List<GameObject> activeEnemies = new List<GameObject>(); // Track active enemies

    private void Start()
    {
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        while (true)
        {
            if (activeEnemies.Count < maxEnemies)
            {
                SpawnWave();
            }
            yield return new WaitForSeconds(waveInterval); // Wait for the next wave
        }
    }

    private void SpawnWave()
    {
        if (spawnPoints.Count < enemiesPerWave)
        {
            Debug.LogError("Not enough spawn points for the number of enemies!");
            return;
        }

        // Select random spawn points for each enemy, ensuring no duplicates
        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        for (int i = 0; i < enemiesPerWave && activeEnemies.Count < maxEnemies; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform spawnPoint = availablePoints[randomIndex];
            availablePoints.RemoveAt(randomIndex); // Ensure no duplicate spawn point

            // Spawn the enemy at the selected spawn point
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            // Add to active enemies list
            activeEnemies.Add(enemy);

            // Attach a listener to remove the enemy from the list when it is destroyed
            Actor actor = enemy.GetComponent<Actor>();
            if (actor != null)
            {
                actor.OnDestroyed += RemoveEnemyFromList;
            }
        }
    }

    private void RemoveEnemyFromList(GameObject enemy)
    {
        activeEnemies.Remove(enemy);
    }

    private void Update()
    {
        // Clean up destroyed enemies automatically
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
}


