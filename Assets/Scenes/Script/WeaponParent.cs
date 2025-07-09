using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponParent : MonoBehaviour
{
    Vector3 _mouseScreenPosition;
    Vector3 _mouseWorldPosition;
    [SerializeField]GameObject[] _weapons;
    public int _equippedMeleeIndex = 2;
    public int _equippedRangeIndex = 2;
    public GameObject[] _equippedWeapons;//���ڴ洢װ����������0����������ս������1��������Զ������
    
    public int _weaponSpawnerIndex = 0;
    public bool _isAttacking { get; private set; } = false; //uesd to control weapon no rotation while melee attacking
    public GameObject _currentWeapon { get; private set; }

    public static event System.Action<GameObject> OnWeaponSpawned;

    void BuyNewWeapon(GameObject shop)
    {
        Counter.CounterType counterType = shop.GetComponent<Counter>()._counterType;
        if (counterType == Counter.CounterType.HP) return;
        ChangeEquippedWeapon((int)counterType);
    }

    void ChangeEquippedWeapon(int index)
    {
        if (index < 2)//Զ������
        {
            _equippedRangeIndex = index;
            _equippedWeapons[1] = _weapons[_equippedRangeIndex];
        }
        else//��ս����
        {
            _equippedMeleeIndex = index;
            _equippedWeapons[0] = _weapons[_equippedMeleeIndex];
        }
    }

    void Awake()
    {
        Player.Instance.OnShoppingEvent += BuyNewWeapon;
    }

    void Start()
    {
        _equippedWeapons = new GameObject[2] { _weapons[_equippedMeleeIndex], _weapons[_equippedRangeIndex] };
        SpawnWeapon(null);
    }

    public void ChangeWeapon(int index)
    {
        _weaponSpawnerIndex = index - 1;
        if (_weaponSpawnerIndex < 0 || _weaponSpawnerIndex >= _equippedWeapons.Length 
            || _equippedWeapons[_weaponSpawnerIndex] == null) return;//δװ���������л�

        Transform lastWeaponTransform = _currentWeapon != null ? _currentWeapon.transform : null;
        if(_currentWeapon != null)
            Destroy(_currentWeapon.gameObject);
        SpawnWeapon(lastWeaponTransform);
    }

    void SpawnWeapon(Transform lastWeapon)
    {
        if (_currentWeapon == null)
        {
            Vector3 weaponPosition = new Vector3(transform.position.x + 0.2f, transform.position.y, transform.position.z);
            _currentWeapon = Instantiate(_equippedWeapons[_weaponSpawnerIndex], weaponPosition, Quaternion.identity, transform);
            OnWeaponSpawned?.Invoke(_currentWeapon);
        }
        else
        {
            Vector3 direction = (lastWeapon.position - transform.position).normalized;
            lastWeapon.position = transform.position + direction * 0.2f; // Adjust the position of the last weapon
            _currentWeapon = Instantiate(_equippedWeapons[_weaponSpawnerIndex], lastWeapon.position, lastWeapon.rotation, transform);
        }
    }

    private void OnDisable()
    {
        Player.Instance.OnShoppingEvent -= BuyNewWeapon;
    }

    void Update()
    {
        if(GameManager._instance._isPause) return; // Pause the weapon aiming if the game is paused
        GetMousePosition();
        AimToMouse();
    }

    void AimToMouse()
    {
        transform.right = (_mouseWorldPosition - transform.position).normalized;
    }

    void GetMousePosition()
    {
        _mouseScreenPosition = Mouse.current.position.ReadValue();
        _mouseWorldPosition = Camera.main.ScreenToWorldPoint(_mouseScreenPosition);
        _mouseWorldPosition.z += 10;
    }
}
