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

    void Awake()
    {
        _popUpTextPrefab = Resources.Load<PopUpText>("PopUpText");
    }

    void AddMaxHealth(GameObject counter)
    {
        if (counter.GetComponent<Counter>()._counterType == Counter.CounterType.HP)
        {
            _maxHealth++;
            _currentHealth = _maxHealth;
        }
    }

    private void Start()
    {
        InitialHealth();
        _enemy = GetComponent<Enemy>();

        if (Player.Instance != null)
        {
            Player.Instance.OnShoppingEvent += AddMaxHealth; // ���� Player �Ĺ����¼�
        }
        else
        {
            Debug.LogError("Player instance not found for Health event subscription.");
        }
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
        if (Player.Instance != null)
        {
            Player.Instance.OnShoppingEvent -= AddMaxHealth;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Check if this Health component belongs to the Player and the scene is "Shop"
        if (Player.Instance != null && gameObject == Player.Instance.gameObject)
        {
            if (scene.name == "Shop")
            {
                InitialHealth(); // Resets _currentHealth to _maxHealth
                _isDead = false;    // Ensure player is not marked as dead
                _isHurt = false;   // Reset hurt state as well
                Debug.Log("Player health and status reset upon entering Shop.");
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
