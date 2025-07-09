using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager _instance { get; private set; }

    public GameObject playerPrefab;
    public GameObject _pauseCanvas;
    public GameObject _dieCanvas;
    public bool _isPause { get; private set; } = false; // 用于跟踪暂停状态
    public AudioSource _deadAudio;

    void Start()
    {
        playerPrefab = GameObject.FindGameObjectWithTag("Player");
    }

    public void PlayerDie()
    {
        _dieCanvas.SetActive(true);
        Time.timeScale = 0;
        _isPause = true;
        _deadAudio.Play();
    }

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
        // 启用所有子物体的 SpriteRenderer
        SpriteRenderer [] temps = playerPrefab.GetComponentsInChildren<SpriteRenderer>();
        foreach (var temp in temps)
        {
            temp.enabled = true; 
        }

        playerPrefab.SetActive(true); // 确保玩家对象在返回商店时处于激活状态
        SceneManager.LoadScene("Shop");
        _pauseCanvas.SetActive(false);
        _dieCanvas.SetActive(false);
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
