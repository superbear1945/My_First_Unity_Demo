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
    WeaponParent _weaponParent;
    Health _health;
    [SerializeField] private LayerMask _solidObjectsLayer;
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

    }

    void Start()
    {
        //Initialize property
        _weapon = GameObject.FindGameObjectWithTag("Weapon");


        //Other Logic
        _mouseLeft.started += MeleeAttack;
        _mouseLeft.started += RangeAttack;
        _mouseLeft.performed += StartAutoFire;
        _mouseLeft.canceled += StopAutoFire;
        _changeWeapon.started += OnChangeWeapon;
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
        if (IsSolidEnemy(direction))
        {
            rb2d.velocity = direction * speed * 0.5f;
        }
        else
        {
            rb2d.velocity = direction * speed;
        }
        _isMove = (rb2d.velocity != Vector2.zero);
        _atr.SetBool("isMove", _isMove);
    }

    bool IsSolidEnemy(Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            _rb2d.position,                   // 射线起点
            direction,                        // 射线方向 (使用标准化的输入方向)
            _raycastDistance,                 // 射线长度
            _solidObjectsLayer                // 图层蒙版 (只检测指定的层)
        );
        if (hit.collider == null)
        {
            return false;
        }
        else if (hit.collider.CompareTag("Enemy"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDisable()
    {
        _mouseLeft.started -= RangeAttack;
        _mouseLeft.started -= MeleeAttack;
        _mouseLeft.performed -= StartAutoFire;
        _mouseLeft.canceled -= StopAutoFire;
        _changeWeapon.performed -= OnChangeWeapon;
    }

    private void OnEnable()
    {
        _health.OnDie += PlayerDie;
        _health.Onhit += PlayerHurt;
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
        Debug.Log("Player is hurt by: " + resource.name);
        // 可以在这里添加受伤的动画或音效逻辑
    }

    private void PlayerDie()
    {
        Debug.Log("Player has died.");
        // 可以在这里添加玩家死亡的动画或音效逻辑
        //_atr.SetBool("isDead", true);
        // 停止玩家的移动
        //_rb2d.velocity = Vector2.zero;
        // 其他死亡处理逻辑，例如重置场景或显示游戏结束界面
    }

    void OnTriggerEnter2D(Collider2D collision)
    {

    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     Gizmos.DrawRay(transform.position, Vector2.right * _raycastDistance);
    // }
}
