using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Awake()
    {
        if (Player.Instance == null)
        {
            // Player instance does not exist, so spawn a new one.
            GameObject playerPrefab = Resources.Load<GameObject>("Player");
            if (playerPrefab != null)
            {
                Instantiate(playerPrefab, transform.position, transform.rotation);
                // The new Player's Awake() method will set Player.Instance.
            }
            else
            {
                Debug.LogError("Player prefab not found in Resources folder. Cannot spawn new player.");
            }
        }
        else
        {
            // Player instance already exists, move it to the spawner's location.
            Player.Instance.transform.position = transform.position;
            Player.Instance.transform.rotation = transform.rotation;
            // Optionally, ensure the player is active if it might have been deactivated.
            // Player.Instance.gameObject.SetActive(true); 
        }
    }
}
