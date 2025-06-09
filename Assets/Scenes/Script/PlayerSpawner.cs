using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Awake()
    {
        GameObject playerPrefab = Resources.Load<GameObject>("Player");
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogError("Player prefab not found in Resources folder.");
        }
    }
}
