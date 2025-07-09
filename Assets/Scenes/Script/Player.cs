using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    InputAction _move;
    InputAction _mouseLeft;
    InputAction _interact;
    InputAction _changeWeapon;
    InputAction _pause;
    
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
    public event Action<GameObject> OnShoppingEvent; // 购物事件 (改为实例事件)
    [SerializeField] GameObject _deadPlayer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // 确保不会执行后续的 Awake 内容
        }


        //Initialize property
        _rb2d = GetComponent<Rigidbody2D>();
        _move = InputSystem.actions.FindAction("Move");
        _mouseLeft = InputSystem.actions.FindAction("Attack");
        _interact = InputSystem.actions.FindAction("Interact");
        _changeWeapon = InputSystem.actions.FindAction("ChangeWeapon");
        _pause = InputSystem.actions.FindAction("Pause");
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
        _interact.started += Shopping;
        _changeWeapon.started += OnChangeWeapon;
        _pause.performed += Pause;
        _health.Onhit += PlayerHurt; // PlayerHurt 现在处理所有受伤逻辑，包括闪烁
        Health.OnDie += PlayerDie;

        WeaponParent.OnWeaponSpawned += (weapon) => _weapon = weapon;//防止游戏刚开始时角色获取不到武器
    }

    private void Pause(InputAction.CallbackContext context)
    {
        if (_health._isDead) return;
        if (GameManager._instance._isPause && !_health._isDead)
        {
            GameManager._instance.PauseBack();
        }
        else if(!_health._isDead)
        {
            GameManager._instance.Pause();
        }
    }

    private void Shopping(InputAction.CallbackContext context)
    {
        float checkRadius = 1.5f; // 可根据需要调整检测半径
        Vector2 playerPos = transform.position;

        // 先用Physics2D.OverlapCircleAll检测所有碰撞体
        Collider2D[] hits = Physics2D.OverlapCircleAll(playerPos, checkRadius);

        foreach (var hit in hits)
        {
            if (hit == null) continue;
            // 检查Layer
            if (LayerMask.LayerToName(hit.gameObject.layer) == "Shop")
            {
                OnShoppingEvent?.Invoke(hit.gameObject);
                return;
            }
            // 检查Tag
            if (hit.CompareTag("Shop"))
            {
                OnShoppingEvent?.Invoke(hit.gameObject);
                return;
            }
        }
        Debug.Log("附近没有商店对象，无法购物。");
    }

    void OnDrawGizmos()//绘制角色可以购买物品的范围
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 1.5f);
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
        Vector2 direction = _move.ReadValue<Vector2>().normalized; // 标准化以获得纯方向
        Vector2 targetVelocity = direction * speed;

        // 分别检测 X 和 Y 轴的墙壁碰撞
        RaycastHit2D hitX = Physics2D.Raycast(_rb2d.position, new Vector2(direction.x, 0), _raycastDistance, _wallLayer);
        if (hitX.collider != null)
        {
            targetVelocity.x = 0; // 如果 X 轴有墙，则 X 轴速度为0
        }

        RaycastHit2D hitY = Physics2D.Raycast(_rb2d.position, new Vector2(0, direction.y), _raycastDistance, _wallLayer);
        if (hitY.collider != null)
        {
            targetVelocity.y = 0; // 如果 Y 轴有墙，则 Y 轴速度为0
        }

        // 如果没有撞到墙，再检测敌人
        if (hitX.collider == null && hitY.collider == null && IsSolidObject(direction, _enemyLayer))
        {
            rb2d.velocity = targetVelocity * 0.3f;
        }
        else
        {
            rb2d.velocity = targetVelocity;
        }

        _isMove = (rb2d.velocity != Vector2.zero);
        _atr.SetBool("isMove", _isMove);
    }

    bool IsSolidObject(Vector2 direction, LayerMask layer)
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
        _interact.started -= Shopping;
        _changeWeapon.performed -= OnChangeWeapon;
        _pause.performed -= Pause;
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
        GameManager._instance.PlayerDie(); // 调用 GameManager 的 PlayerDie 方法
        Instantiate(_deadPlayer, transform.position, Quaternion.identity); // 实例化死亡角色
        SpriteRenderer [] temps = GetComponentsInChildren<SpriteRenderer>();
        foreach (var temp in temps)
        {
            temp.enabled = false; // 禁用所有子物体的 SpriteRenderer
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            _health.CauseDamage(1, collision.gameObject);
        }
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Debug.Log("performed");
        }
    }
}
