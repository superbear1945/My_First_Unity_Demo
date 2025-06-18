using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using System.Timers;
using UnityEngine.SceneManagement;

public class Level2Manager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField]private float _timeValue = 100f;
    public float _time
    {
        get { return _timeValue; }
        set
        {
            _timeValue = value;
            if (_timeText != null)
            {
                _timeText.text = _timeValue.ToString("F2");
            }
        }
    }

    void Update()
    {
        _time -= Time.deltaTime;
        if (_time <= 0)
        {
            SceneManager.LoadScene("Shop");//Remember to change the scene name to the actual scene name
        }
    }
}
