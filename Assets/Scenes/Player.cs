using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    Rigidbody2D _rb2d;
    SpriteRenderer _sr;
    public float _walkSpeed = 10;
    Vector3 _mouseScreenPosition;
    Vector3 _mouseWorldPosition;
    GameObject _weapon;
    Animator _atr;
    public bool _isMove;
    WeaponParent _weaponParent;
    
    void Start()
    {
        //Initialize property
        _rb2d = GetComponent<Rigidbody2D>();
        _move = InputSystem.actions.FindAction("Move");
        _mouseLeft = InputSystem.actions.FindAction("Attack");
        _changeWeapon = InputSystem.actions.FindAction("ChangeWeapon");
        _sr = GetComponent<SpriteRenderer>();
        _weapon = GameObject.FindGameObjectWithTag("Weapon");
        _atr = GetComponent<Animator>();
        _weaponParent = GetComponentInChildren<WeaponParent>();

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


    private void _mouseLeft_canceled(InputAction.CallbackContext obj)
    {
        throw new NotImplementedException();
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
        Vector2 dirction = _move.ReadValue<Vector2>();
        rb2d.velocity = dirction * speed;
        _isMove = (rb2d.velocity != Vector2.zero);
        _atr.SetBool("isMove", _isMove);
    }

    private void OnDisable()
    {
        _mouseLeft.started -= RangeAttack;
        _mouseLeft.started -= MeleeAttack;
        _mouseLeft.performed -= StartAutoFire;
        _mouseLeft.canceled -= StopAutoFire;
        _changeWeapon.performed -= OnChangeWeapon;
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
        _weapon.GetComponent<SpriteRenderer>().flipY = _mouseWorldPosition.x < transform.position.x;
        bool isFlipped = _mouseWorldPosition.x < transform.position.x;
        if(_weapon.GetComponent<RangeWeapon>() != null || _weapon.GetComponent<AutoRangeWeapon>() != null)//ֻ��Զ��������Ҫ���·�ת
        {
            _weapon.GetComponent<SpriteRenderer>().flipY = isFlipped; _weapon.GetComponent<Animator>().SetBool("isFlipped", isFlipped);
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
}
