using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager _instance { get; private set; }

    [SerializeField] private int coinValue = 0;
    public int _coin
    {
        get { return coinValue; }
        set
        {
            coinValue = value;
            if (coinText != null)
            {
                coinText.text = coinValue.ToString();
            }
        }
    }
    [SerializeField] private TextMeshProUGUI coinText;

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

        // Initialize coin text on Awake
        if (coinText != null)
        {
            coinText.text = coinValue.ToString();
        }
    }

    void Start()
    {
        Health.OnDie += AddCoin;
    }

    void OnDisable()
    {
        Health.OnDie -= AddCoin;
    }

    void AddCoin(Health healthInstance)
    {
        if (healthInstance != null && healthInstance.gameObject.CompareTag("Enemy"))
        {
            _coin++;
            
        }
    }
}
