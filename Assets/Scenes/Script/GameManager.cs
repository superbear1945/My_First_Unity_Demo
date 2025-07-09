using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }

    public GameObject playerPrefab;
    public GameObject _pauseCanvas;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackToShop()
    {
        SceneManager.LoadScene("Shop");
        _pauseCanvas.SetActive(false);
    }

    public void Pause()
    {
        Time.timeScale = 0;
        _pauseCanvas.SetActive(true);
    }
}
