using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner : MonoBehaviour
{
    Health []_healths;
    [SerializeField] int _enemyCount = 10;
    [SerializeField] float _minRadius = 10f; 
    [SerializeField] float _maxRadius = 15f; 
    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] LayerMask _wallLayer; // Layer for walls to avoid spawning behind them

    private Transform _playerTransform;

    void Awake()
    {
        // Ensure subscription happens only once
        // Health.OnDie -= StepToLevel3; // Remove previous subscriptions if any (safety measure)
        // Health.OnDie += StepToLevel3;
    }

    void Start()
    {
        //Initialize parameters
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        _healths = new Health[enemies.Length]; // This array might still be useful for other direct interactions if needed
        for (int i = 0; i < enemies.Length; i++)
        {
            _healths[i] = enemies[i].GetComponent<Health>();
            // Health.OnDie += StepToLevel2; // Moved to Awake to subscribe only once
        }
        // Update initial enemy count based on spawned enemies or a set value
        // If _enemyCount is meant to be the number of enemies to defeat, 
        // it should be initialized correctly, e.g., based on FindGameObjectsWithTag("Enemy").Length or a serialized value.
        // For now, I assume _enemyCount is correctly initialized as a serialized field representing enemies to defeat.
        
        SpawnEnemiesAtStart();
    }

    void OnDestroy() // It's good practice to unsubscribe from static events when the listener is destroyed
    {
        // Health.OnDie -= StepToLevel3;
    }
    

    public void SpawnEnemiesAtStart()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = playerObject.transform;

        int spawnedCount = 0;
        int attempts = 0;              // To prevent infinite loops if valid positions are hard to find
        int maxAttemptsPerEnemy = 100; // Max attempts to find a spot for a single enemy

        while (spawnedCount < _enemyCount && attempts < _enemyCount * maxAttemptsPerEnemy)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomRadius = Random.Range(_minRadius, _maxRadius);

            // For 2D top-down (XY plane)
            Vector2 randomDirection2D = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
            // Assuming player's Z position should be maintained for the spawned enemy, or it's irrelevant for 2D.
            Vector3 potentialSpawnPoint = _playerTransform.position + new Vector3(randomDirection2D.x, randomDirection2D.y, 0) * randomRadius;
            // Ensure Z is same as player if that's important, or set to a fixed Z for 2D plane.
            potentialSpawnPoint.z = _playerTransform.position.z;


            // Check if there's a wall between the player and the potential spawn point using Physics2D
            // Convert player position and potential spawn point to Vector2 for Physics2D.Raycast
            Vector2 playerPosition2D = new Vector2(_playerTransform.position.x, _playerTransform.position.y);
            Vector2 potentialSpawnPoint2D = new Vector2(potentialSpawnPoint.x, potentialSpawnPoint.y);
            Vector2 directionToPotentialPoint2D = (potentialSpawnPoint2D - playerPosition2D).normalized;
            float distanceToPotentialPoint2D = Vector2.Distance(playerPosition2D, potentialSpawnPoint2D);

            RaycastHit2D hit = Physics2D.Raycast(playerPosition2D, directionToPotentialPoint2D, distanceToPotentialPoint2D, _wallLayer);

            if (hit.collider == null) // If the raycast doesn't hit anything on the _wallLayer
            {
                // No wall in between, valid spawn point
                Instantiate(_enemyPrefab, potentialSpawnPoint, Quaternion.identity);
                spawnedCount++;
            }
            attempts++;
        }

        if (spawnedCount < _enemyCount)
        {
            Debug.LogWarning($"Failed to spawn all enemies. Spawned {spawnedCount} out of {_enemyCount} requested.");
        }
    }

    private void StepToLevel3(Health healthInstance)
    {
        // Check if the Health component that died belongs to an Enemy
        if (healthInstance != null && healthInstance.CompareTag("Enemy"))
        {
            _enemyCount--;
            if (_enemyCount <= 0)
            {
                SceneManager.LoadScene("Level2");
            }
        }
    }
}
