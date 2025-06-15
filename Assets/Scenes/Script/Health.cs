using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int _maxHealth = 1;
    [SerializeField]public int _currentHealth { get; private set; } = 1;
    public static event Action<Health> OnDie;
    public event Action<GameObject> Onhit;
    public bool _isDead { get; private set; }
    public bool _isHurt { get; private set; }
    Enemy _enemy;

    void SetHealth(GameObject counter)
    {
        if(counter.GetComponent<Counter>()._counterType == Counter.CounterType.HP)
            _maxHealth++;
    }

    private void Start()
    {
        InitialHealth();
        _enemy = GetComponent<Enemy>();

        Player.OnShopping += SetHealth; // ���� Player �Ĺ����¼�
    }

    void OnDisable()
    {
        Player.OnShopping -= SetHealth;
    }

    private void InitialHealth()
    {
        _currentHealth = _maxHealth;
    }

    public void CauseDamage(int damage, GameObject resource)
    {
        if (_isHurt) return;
        _currentHealth -= damage;
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
