using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Enemy : MonoBehaviour
{
    Animator _animator;
    Health _health;
    Collider2D _collider2D;
    [SerializeField] AudioSource _meleeHitAS;
    [SerializeField] AudioSource _rangeHitAS;
    [SerializeField] AudioSource _dieAS;
    GameObject _player;
    Rigidbody2D _rb2d;
    float _visionRange = 10f; //视野范围
    public float _moveSpeed = 10f; //移动速度
    SpriteRenderer _spriteRenderer;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _collider2D = GetComponent<Collider2D>();
        _health.OnDie += Die;
        _health.Onhit += Hurt;
        _rb2d = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Awake()
    {
        
    }

    public void Hurt(GameObject resource)//播放伤害音效并触发受伤动画
    {
        if (resource.TryGetComponent<Bullet>(out Bullet bullet))
            _rangeHitAS.Play();
        else if (resource.TryGetComponent<MeleeWeapon>(out MeleeWeapon meleeWeapon))
            _meleeHitAS.Play();
        _animator.SetTrigger("getDamage");
    }

    public void Die()
    {
        _collider2D.enabled = false;
        _dieAS.Play();
        _animator.SetBool("isDead", true);
        _spriteRenderer.sortingOrder = 9;
        StartCoroutine(DieDelay());
    }

    private IEnumerator DieDelay()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        MoveTowardsPlayer(_moveSpeed, GetComponent<Health>()._isHurt, _player, _visionRange, _animator);
    }

    private void MoveTowardsPlayer(float speed, bool isHurt, GameObject player, float range, Animator animator)
    {
        if (_health._isDead)// 如果敌人已经死亡，则不执行移动逻辑，并且速度归零
        {
            _rb2d.velocity = Vector2.zero;
            return;
        }
        if (isHurt) return; // 如果敌人受伤，则不执行移动逻辑
        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= range)
        {
            Vector2 direction = (player.transform.position - transform.position).normalized;
            Vector2 targetPosition = direction * speed * Time.fixedDeltaTime + _rb2d.position;
            _rb2d.MovePosition(targetPosition);
            animator.SetBool("isMove", true);
            FlipSprite(direction.x > 0, _spriteRenderer);
        }
        else
        {
            animator.SetBool("isMove", false);
        }
    }

    private void FlipSprite(bool isFacingRight, SpriteRenderer sprite)
    {
        sprite.flipX = !isFacingRight;
    }

    
}
