using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }

    public GameObject playerPrefab;
    public GameObject _pauseCanvas;
    public bool _isPause { get; private set; } = false; // 用于跟踪暂停状态

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
        Time.timeScale = 1;
        _isPause = false;
    }

    public void Pause()
    {
        Time.timeScale = 0;
        _pauseCanvas.SetActive(true);
        _isPause = true;
    }

    public void PauseBack()
    {
        Time.timeScale = 1;
        _pauseCanvas.SetActive(false);
        _isPause = false;
    }
}
