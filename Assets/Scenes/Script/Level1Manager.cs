using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Level1Manager : MonoBehaviour
{
    Health []_healths;
    [SerializeField] int _enemyCount = 5;
    [SerializeField] float _minRadius = 10f; 
    [SerializeField] float _maxRadius = 20f; 
    [SerializeField] GameObject _enemyPrefab;
    [SerializeField] LayerMask _wallLayer; // Layer for walls to avoid spawning behind them

    private Transform _playerTransform;

    void Awake()
    {
        SpawnEnemies(); 
    }

    void Start()
    {
        //Initialize parameters
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        _healths = new Health[enemies.Length];
        for (int i = 0; i < enemies.Length; i++)
        {
            _healths[i] = enemies[i].GetComponent<Health>();
            _healths[i].OnDie += StepToLevel2;
            Debug.Log(enemies[i].name);
        }

        
    }

    

    public void SpawnEnemies()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        _playerTransform = playerObject.transform;
        if (_playerTransform == null)
        {
            Debug.LogError("Player transform is not set. Cannot spawn enemies.");
            return;
        }
        if (_enemyPrefab == null)
        {
            Debug.LogError("Enemy prefab is not set. Cannot spawn enemies.");
            return;
        }

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

    private void StepToLevel2()
    {
        _enemyCount--;
        if(_enemyCount <= 0) SceneManager.LoadScene("Level2");
    }
}
