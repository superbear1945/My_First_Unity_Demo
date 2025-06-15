using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    GameObject _weapon;
    Rigidbody2D _rb2d;
    [SerializeField]float _destroyTime = 5f;
    public float _speed = 20f;
    public int _damage = 1;
    private int _hitEnemyCount = 0; // 用于记录击中敌人的次数
    // Start is called before the first frame update
    void Start()
    {
        //Initialize property
        _weapon = GameObject.FindGameObjectWithTag("Weapon");
        _rb2d = GetComponent<Rigidbody2D>();

        //Other logic
        transform.up = _weapon.transform.right;
        Fire();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void Fire()
    {
        _rb2d.velocity = transform.up * _speed;
        StartCoroutine(DestroyBullet());
    }

    private IEnumerator DestroyBullet()
    {
        yield return new WaitForSeconds(_destroyTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy") && !collision.CompareTag("Player"))
        {
            Destroy(gameObject);
            return;
        }
        Health enemy = collision.GetComponent<Health>();
        enemy.CauseDamage(_damage, gameObject);
        _hitEnemyCount++;
        if (_hitEnemyCount >= 3)
        {
            Destroy(gameObject);
        }
    }
}
