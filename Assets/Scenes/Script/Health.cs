using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Required for SceneManager

public class Health : MonoBehaviour
{
    public int _maxHealth = 1;
    [SerializeField]public int _currentHealth { get; private set; } = 1;
    public static event Action<Health> OnDie;
    public event Action<GameObject> Onhit;
    public bool _isDead { get; private set; }
    public bool _isHurt { get; private set; }
    Enemy _enemy;
    PopUpText _popUpTextPrefab;
    GameObject _hpCounter;

    void Awake()
    {
        _popUpTextPrefab = Resources.Load<PopUpText>("PopUpText");
    }

    void AddMaxHealth()
    {
        Debug.Log(gameObject.CompareTag("Player"));
        if (gameObject.CompareTag("Player") == false) return; //不是玩家不加生命值
        _maxHealth++;
        _currentHealth = _maxHealth;
        Debug.Log("Hp++");
    }

    private void Start()
    {
        InitialHealth();
        _enemy = GetComponent<Enemy>();
        // _hpCounter = GameObject.FindGameObjectWithTag("HpShop");
        // if(_hpCounter != null) _hpCounter.GetComponent<Counter>().OnShoppingEvent += AddMaxHealth;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // Note: The subscription to Player.Instance.OnShoppingEvent happens in Start, 
        // so OnEnable/OnDisable for SceneManager events is fine here.
        // If OnShoppingEvent subscription was also in OnEnable, we'd combine them.
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if(_hpCounter != null) _hpCounter.GetComponent<Counter>().OnShoppingEvent -= AddMaxHealth;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if this Health component belongs to the Player and the scene is "Shop"
        if (Player.Instance != null && gameObject == Player.Instance.gameObject)
        {
            if (scene.name == "Shop") //回到了Shop关卡
            {
                InitialHealth();   // Resets _currentHealth to _maxHealth
                _isDead = false;   // Ensure player is not marked as dead
                _isHurt = false;   // Reset hurt state as well
                _hpCounter = GameObject.FindGameObjectWithTag("HpShop");
                if(_hpCounter != null) _hpCounter.GetComponent<Counter>().OnShoppingEvent += AddMaxHealth;
            }
        }
    }

    private void InitialHealth()
    {
        _currentHealth = _maxHealth;
    }

    public void CauseDamage(int damage, GameObject resource)
    {
        if (_isHurt) return;
        _currentHealth -= damage;
        if(_currentHealth >= 1)
            _popUpTextPrefab.Create(damage, gameObject); //用于弹出伤害数字，致死伤害不弹数字
        if (_currentHealth <= 0 && !_isDead)
        {

            _isDead = true;
            OnDie?.Invoke(this);
            return;
        }
        Onhit?.Invoke(resource);
        _isHurt = true;
    }

    public void ResetIsHurt()
    {
        _isHurt = false;
    }
}
