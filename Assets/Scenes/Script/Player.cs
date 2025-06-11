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
    [SerializeField] private float _hurtFlashTotalDuration = 0.5f; // �ɵ�����������˸��ʱ��
    [SerializeField] private int _hurtFlashCount = 3; // �ɵ�����������˸����

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
        _health.Onhit += PlayerHurt; // PlayerHurt ���ڴ������������߼���������˸
        Health.OnDie += PlayerDie;

        WeaponParent.OnWeaponSpawned += (weapon) => _weapon = weapon;//��ֹ��Ϸ�տ�ʼʱ��ɫ��ȡ��������
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
        if (IsSolidEnemy(direction, _enemyLayer))
        {
            rb2d.velocity = direction * speed * 0.3f;
        }
        else if (IsSolidEnemy(direction, _wallLayer))//ײ��ǽͣ��
        {
            RaycastHit2D hit = Physics2D.Raycast(
            _rb2d.position,
            direction,
            _raycastDistance,
            _wallLayer);
            Vector2 wallNormal = hit.normal; // ��ȡǽ�ڵķ���
            Vector2 projectedDirection = Vector2.Perpendicular(wallNormal); // ������ǽ��ƽ�еķ���
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
            _rb2d.position,                   // �������
            direction,                        // ���߷��� (ʹ�ñ�׼�������뷽��)
            _raycastDistance,                 // ���߳���
            layer                             // ͼ���ɰ� (ֻ���ָ���Ĳ�)
        );
        return hit.collider != null;//������������򲻼���ǰ��
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
        // ������������Ӷ�������˶�������Ч�߼�
    }

    // FlashSpriteAndResetHurtState Э�̴�����˸Ч�����ڽ��������� _isHurt
    private IEnumerator FlashSpriteAndResetHurtState(float totalDuration, int flashCount)
    {
        Color originalColor = _sr.color;
        Color flashColorRed = Color.red;
        Color flashColorWhite = Color.white;

        if (flashCount <= 0) flashCount = 1; // ��ֹ��Ч����˸����
        if (totalDuration <= 0) totalDuration = 0.1f; // ��ֹ��Ч����ʱ��

        // segmentDuration ��ָ����״̬�ĳ���ʱ��
        float segmentDuration = totalDuration / (flashCount * 2);

        for (int i = 0; i < flashCount; i++)
        {
            _sr.color = flashColorRed;
            yield return new WaitForSeconds(segmentDuration);
            _sr.color = flashColorWhite;
            yield return new WaitForSeconds(segmentDuration);
        }
        _sr.color = originalColor; // ȷ�����ָ�ԭɫ
        _health.ResetIsHurt(); // ����˸������ȫ���������� _isHurt ״̬
    }

    private void PlayerDie(Health healthInstance)
    {
        if (healthInstance != _health) return; // ����������� Health �������ʱ����Ӧ
        Debug.Log("Player has died.");
        // ���������������������Ķ�������Ч�߼�
        //_atr.SetBool("isDead", true);
        // ֹͣ��ҵ��ƶ�
        //_rb2d.velocity = Vector2.zero;
        // �������������߼����������ó�������ʾ��Ϸ��������
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
