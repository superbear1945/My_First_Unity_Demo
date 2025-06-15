using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class Enemy : MonoBehaviour
{
    protected Animator _animator;
    protected Health _health;
    Collider2D _collider2D; // Remains private as it's not directly used in MoveTowardsPlayer by Enemy1
    [SerializeField] AudioSource _meleeHitAS;
    [SerializeField] AudioSource _rangeHitAS;
    [SerializeField] AudioSource _dieAS;
    protected GameObject _playerGameObject; // Renamed for clarity
    protected Rigidbody2D _rb2d;
    float _visionRange = 10f; //视野范围
    public float _moveSpeed = 10f; //移动速度
    protected SpriteRenderer _spriteRenderer;

    void Start()
    {
        if (Player.Instance != null)
        {
            _playerGameObject = Player.Instance.gameObject;
        }
        else
        {
            Debug.LogError("Player instance not found in Enemy.");
        }
        _animator = GetComponent<Animator>();
        _health = GetComponent<Health>();
        _collider2D = GetComponent<Collider2D>();
        Health.OnDie += Die;
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

    public void Die(Health healthInstance)
    {
        if (healthInstance != _health) return; // 仅当是自身的 Health 组件触发时才响应
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
        MoveTowardsPlayer(_moveSpeed, GetComponent<Health>()._isHurt, _playerGameObject, _visionRange, _animator);
    }

    protected virtual void MoveTowardsPlayer(float speed, bool isHurt, GameObject playerGameObject, float range, Animator animator)
    {
        if (_health._isDead)// 如果敌人已经死亡，则不执行移动逻辑，并且速度归零
        {
            _rb2d.velocity = Vector2.zero;
            return;
        }
        if (isHurt) return; // 如果敌人受伤，则不执行移动逻辑
        if (playerGameObject != null && Vector2.Distance(transform.position, playerGameObject.transform.position) <= range)
        {
            Vector2 direction = (playerGameObject.transform.position - transform.position).normalized;
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

    protected void FlipSprite(bool isFacingRight, SpriteRenderer sprite)
    {
        sprite.flipX = !isFacingRight;
    }

    
}
