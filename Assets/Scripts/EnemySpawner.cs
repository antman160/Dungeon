using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // Prefab of the enemy to spawn
    public float waveInterval = 10f; // Time between waves (in seconds)
    public int enemiesPerWave = 3; // Default number of enemies per wave
    public int maxEnemies = 10; // Maximum number of active enemies allowed
    public Transform player; // Reference to the player transform
    public float maxSpawnDistance = 10f; // Max distance from the player
    public float playerFov = 90f; // Player's field of view

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
        List<Transform> spawnPoints = new List<Transform>();
        foreach (GameObject spawnObj in GameObject.FindGameObjectsWithTag("spawnPoint"))
        {
            spawnPoints.Add(spawnObj.transform);
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points found with the tag 'spawnPoint'!");
            return;
        }

        List<Transform> validSpawnPoints = GetValidSpawnPoints(spawnPoints);

        if (validSpawnPoints.Count < enemiesPerWave)
        {
            Debug.LogWarning("Not enough valid spawn points!");
            return;
        }

        for (int i = 0; i < enemiesPerWave && activeEnemies.Count < maxEnemies; i++)
        {
            Transform selectedPoint = GetFurthestSpawnPoint(validSpawnPoints);
            validSpawnPoints.Remove(selectedPoint);

            GameObject enemy = Instantiate(enemyPrefab, selectedPoint.position, selectedPoint.rotation);
            activeEnemies.Add(enemy);

            Actor actor = enemy.GetComponent<Actor>();
            if (actor != null)
            {
                actor.OnDestroyed += RemoveEnemyFromList;
            }
        }
    }

    private List<Transform> GetValidSpawnPoints(List<Transform> spawnPoints)
    {
        List<Transform> validPoints = new List<Transform>();

        foreach (Transform spawnPoint in spawnPoints)
        {
            float distanceToPlayer = Vector3.Distance(player.position, spawnPoint.position);

            if (distanceToPlayer <= maxSpawnDistance && !IsInPlayerFov(spawnPoint))
            {
                validPoints.Add(spawnPoint);
            }
        }

        return validPoints;
    }

    private Transform GetFurthestSpawnPoint(List<Transform> points)
    {
        Transform furthestPoint = null;
        float maxDistance = 0f;

        foreach (Transform point in points)
        {
            float distance = Vector3.Distance(player.position, point.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                furthestPoint = point;
            }
        }

        return furthestPoint;
    }

    private bool IsInPlayerFov(Transform point)
    {
        Vector3 directionToPoint = (point.position - player.position).normalized;
        float angle = Vector3.Angle(player.forward, directionToPoint);
        return angle <= playerFov / 2f;
    }

    private void RemoveEnemyFromList(GameObject enemy)
    {
        if (enemy != null && activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    private void Update()
    {
        // Clean up destroyed enemies automatically
        activeEnemies.RemoveAll(enemy => enemy == null);
    }
}
