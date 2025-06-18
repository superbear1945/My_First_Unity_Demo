using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager _instance { get; private set; }

    [SerializeField] private int coinValue = 0;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private Scrollbar _healthScrollbar;
    [SerializeField] private TextMeshProUGUI _maxHealthText;
    [SerializeField] private TextMeshProUGUI _currentHealthText;
    Health _playerHealth;
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

    void ChangeHealthBar(GameObject resource) //����ʱѪ������
    {
        if (_playerHealth != null && _playerHealth._isDead)
        {
            // ����������������˷�����Ӧ�ٸ��������������Ѫ��
            // ����ʱ��Ѫ��״̬�� ChangeHealthBar(Health resource) ȫȨ����
            return;
        }
        _healthScrollbar.size = (float)_playerHealth._currentHealth / _playerHealth._maxHealth;
        Debug.Log("Hurt");
    }

    void ChangeHealthBar(Health resource) //����ʱ����Ѫ��
    {
        if (resource != null && resource.gameObject.CompareTag("Player"))
        {
            _healthScrollbar.size = 1;
            Debug.Log("Player Die"); //�ܹ�ʵ��
        }
    }

    void Update()
    {
        ChangeHealthText();
    }

    void ChangeHealthText()
    {
        _currentHealthText.text = _playerHealth._currentHealth.ToString();
        _maxHealthText.text = _playerHealth._maxHealth.ToString();
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

        _playerHealth = Player.Instance.GetComponent<Health>();
        _playerHealth.Onhit += ChangeHealthBar;
        Health.OnDie += ChangeHealthBar;
        ChangeHealthBar((GameObject)null);

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
