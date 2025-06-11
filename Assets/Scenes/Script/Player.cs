using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    InputAction _move;
    InputAction _mouseLeft;
    InputAction _changeWeapon;
    [SerializeField] Rigidbody2D _rb2d;
    SpriteRenderer _sr;
    public float _walkSpeed = 10;
    Vector3 _mouseScreenPosition;
    Vector3 _mouseWorldPosition;
    GameObject _weapon;
    Animator _atr;
    public bool _isMove;
    AudioSource _hurtSound;
    WeaponParent _weaponParent;
    Health _health;
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private LayerMask _wallLayer;
    [SerializeField] private float _hurtFlashTotalDuration = 0.5f; // 可调整的受伤闪烁总时间
    [SerializeField] private int _hurtFlashCount = 3; // 可调整的受伤闪烁次数

    [SerializeField] private float _raycastDistance = 0.1f; // 射线检测的距离

    void Awake()
    {
        //Initialize property
        _rb2d = GetComponent<Rigidbody2D>();
        _move = InputSystem.actions.FindAction("Move");
        _mouseLeft = InputSystem.actions.FindAction("Attack");
        _changeWeapon = InputSystem.actions.FindAction("ChangeWeapon");
        _sr = GetComponent<SpriteRenderer>();
        _atr = GetComponent<Animator>();
        _weaponParent = GetComponentInChildren<WeaponParent>();
        _health = GetComponent<Health>();
        _hurtSound = GetComponent<AudioSource>();

    }

    void Start()
    {
        //Other Logic
        _mouseLeft.started += MeleeAttack;
        _mouseLeft.started += RangeAttack;
        _mouseLeft.performed += StartAutoFire;
        _mouseLeft.canceled += StopAutoFire;
        _changeWeapon.started += OnChangeWeapon;
        _health.Onhit += PlayerHurt; // PlayerHurt 现在处理所有受伤逻辑，包括闪烁
        Health.OnDie += PlayerDie;

        WeaponParent.OnWeaponSpawned += (weapon) => _weapon = weapon;//防止游戏刚开始时角色获取不到武器
    }

    void OnChangeWeapon(InputAction.CallbackContext context)//切换武器事件的回调函数
    {
        // 如果没有找到WeaponManager的引用，则直接返回，防止报错
        if (_weaponParent == null) return;

        // 获取按键的名称 (例如 "1", "2")
        string controlName = context.control.name;

        // 尝试将按键名称字符串转换为整数
        if (int.TryParse(controlName, out int weaponIndex))
        {
            // 如果转换成功，调用WeaponManager中的方法，并传入转换后的整数
            _weaponParent.ChangeWeapon(weaponIndex);
            _weapon = _weaponParent._currentWeapon; // 更新当前武器引用
            Debug.Log(weaponIndex);
        }
        else
        {
            // 如果绑定的按键不是数字（例如 "E" 键），则会转换失败
            Debug.LogWarning($"按键 '{controlName}' 无法被解析为武器索引。");
        }
    }

    void Update()
    {
        FlipPlayerAndWeapon();
    }

    private void FixedUpdate()
    {
        GetMousePosition();
        Move(_rb2d, _walkSpeed);
    }

    private void Move(Rigidbody2D rb2d, float speed)
    {
        Vector2 direction = _move.ReadValue<Vector2>();
        if (IsSolidEnemy(direction, _enemyLayer))
        {
            rb2d.velocity = direction * speed * 0.3f;
        }
        else if (IsSolidEnemy(direction, _wallLayer))//撞到墙停下
        {
            RaycastHit2D hit = Physics2D.Raycast(
            _rb2d.position,
            direction,
            _raycastDistance,
            _wallLayer);
            Vector2 wallNormal = hit.normal; // 获取墙壁的法线
            Vector2 projectedDirection = Vector2.Perpendicular(wallNormal); // 计算与墙壁平行的方向
            if (wallNormal.x == 0) rb2d.velocity = new Vector2(direction.x, 0) * speed;
            else rb2d.velocity = new Vector2(0, direction.y) * speed;
        }
        else
        {
            rb2d.velocity = direction * speed;
        }
        _isMove = (rb2d.velocity != Vector2.zero);
        _atr.SetBool("isMove", _isMove);
    }

    bool IsSolidEnemy(Vector2 direction, LayerMask layer)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            _rb2d.position,                   // 射线起点
            direction,                        // 射线方向 (使用标准化的输入方向)
            _raycastDistance,                 // 射线长度
            layer                             // 图层蒙版 (只检测指定的层)
        );
        return hit.collider != null;//如果碰到物体则不继续前进
    }



    private void OnDisable()
    {
        _mouseLeft.started -= RangeAttack;
        _mouseLeft.started -= MeleeAttack;
        _mouseLeft.performed -= StartAutoFire;
        _mouseLeft.canceled -= StopAutoFire;
        _changeWeapon.performed -= OnChangeWeapon;
        _health.Onhit -= PlayerHurt;
        Health.OnDie -= PlayerDie;
        WeaponParent.OnWeaponSpawned -= (weapon) => _weapon = weapon;
    }

    void GetMousePosition()
    {
        _mouseScreenPosition = Mouse.current.position.ReadValue();
        _mouseWorldPosition = Camera.main.ScreenToWorldPoint(_mouseScreenPosition);
        _mouseWorldPosition.z += 10;
    }

    void MeleeAttack(InputAction.CallbackContext context)
    {
        if (_weapon == null || _weapon.GetComponent<MeleeWeapon>() == null)
            return;
        MeleeWeapon weapon = _weapon.GetComponent<MeleeWeapon>();
        weapon.MeleeAttack();
    }

    void FlipPlayerAndWeapon()
    {
        _sr.flipX = (_mouseWorldPosition.x < transform.position.x);
        if (_weapon == null) return;
        _weapon.GetComponent<SpriteRenderer>().flipY = _mouseWorldPosition.x < transform.position.x;
        bool isFilpped = _mouseWorldPosition.x < transform.position.x;
        if (_weapon.GetComponent<RangeWeapon>() != null || _weapon.GetComponent<AutoRangeWeapon>() != null)
        {
            _weapon.GetComponent<SpriteRenderer>().flipY = isFilpped;
            _weapon.GetComponent<Animator>().SetBool("isFlipped", isFilpped);
        }
    }

    private void RangeAttack(InputAction.CallbackContext context)
    {
        if (_weapon == null || _weapon.GetComponent<RangeWeapon>() == null)
            return;
        _weapon.GetComponent<Animator>().SetTrigger("Fire");
    }

    private void StartAutoFire(InputAction.CallbackContext context)
    {
        if (_weapon == null || _weapon.GetComponent<AutoRangeWeapon>() == null) return;
        AutoRangeWeapon weapon = _weapon.GetComponent<AutoRangeWeapon>();
        weapon.StartAutoFire();
    }

    private void StopAutoFire(InputAction.CallbackContext context)
    {
        if (_weapon == null || _weapon.GetComponent<AutoRangeWeapon>() == null) return;
        AutoRangeWeapon weapon = _weapon.GetComponent<AutoRangeWeapon>();
        weapon.StopAutoFire();
    }

    private void PlayerHurt(GameObject resource)
    {
        _hurtSound.Play();
        StartCoroutine(FlashSpriteAndResetHurtState(_hurtFlashTotalDuration, _hurtFlashCount));
        // 可以在这里添加额外的受伤动画或音效逻辑
    }

    // FlashSpriteAndResetHurtState 协程处理闪烁效果并在结束后重置 _isHurt
    private IEnumerator FlashSpriteAndResetHurtState(float totalDuration, int flashCount)
    {
        Color originalColor = _sr.color;
        Color flashColorRed = Color.red;
        Color flashColorWhite = Color.white;

        if (flashCount <= 0) flashCount = 1; // 防止无效的闪烁次数
        if (totalDuration <= 0) totalDuration = 0.1f; // 防止无效的总时长

        // segmentDuration 是指红或白状态的持续时间
        float segmentDuration = totalDuration / (flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            _sr.color = flashColorRed;
            yield return new WaitForSeconds(segmentDuration);
            _sr.color = flashColorWhite;
            yield return new WaitForSeconds(segmentDuration);
        }
        _sr.color = originalColor; // 确保最后恢复原色
        _health.ResetIsHurt(); // 在闪烁动画完全结束后重置 _isHurt 状态
    }

    private void PlayerDie(Health healthInstance)
    {
        if (healthInstance != _health) return; // 仅当是自身的 Health 组件触发时才响应
        Debug.Log("Player has died.");
        // 可以在这里添加玩家死亡的动画或音效逻辑
        //_atr.SetBool("isDead", true);
        // 停止玩家的移动
        //_rb2d.velocity = Vector2.zero;
        // 其他死亡处理逻辑，例如重置场景或显示游戏结束界面
        SceneManager.LoadScene("Shop");
    }


    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            _health.CauseDamage(1, collision.gameObject);
            Debug.Log(_health._currentHealth);
        }
    }
}
