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
    [SerializeField] private float _raycastDistance = 0.1f; // ���߼��ľ���

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

    void OnChangeWeapon(InputAction.CallbackContext context)//�л������¼��Ļص�����
    {
        // ���û���ҵ�WeaponManager�����ã���ֱ�ӷ��أ���ֹ����
        if (_weaponParent == null) return;

        // ��ȡ���������� (���� "1", "2")
        string controlName = context.control.name;

        // ���Խ����������ַ���ת��Ϊ����
        if (int.TryParse(controlName, out int weaponIndex))
        {
            // ���ת���ɹ�������WeaponManager�еķ�����������ת���������
            _weaponParent.ChangeWeapon(weaponIndex);
            _weapon = _weaponParent._currentWeapon; // ���µ�ǰ��������
            Debug.Log(weaponIndex);
        }
        else
        {
            // ����󶨵İ����������֣����� "E" ���������ת��ʧ��
            Debug.LogWarning($"���� '{controlName}' �޷�������Ϊ����������");
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
            _rb2d.position,                   // �������
            direction,                        // ���߷��� (ʹ�ñ�׼�������뷽��)
            _raycastDistance,                 // ���߳���
            _solidObjectsLayer                // ͼ���ɰ� (ֻ���ָ���Ĳ�)
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
        // ����������������˵Ķ�������Ч�߼�
    }

    private void PlayerDie()
    {
        Debug.Log("Player has died.");
        // ���������������������Ķ�������Ч�߼�
        //_atr.SetBool("isDead", true);
        // ֹͣ��ҵ��ƶ�
        //_rb2d.velocity = Vector2.zero;
        // �������������߼����������ó�������ʾ��Ϸ��������
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
