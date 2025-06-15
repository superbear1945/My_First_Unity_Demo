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
    public GameObject[] _equippedWeapons;//用于存储装备的武器，0索引代表近战武器，1索引代表远程武器
    
    public int _weaponSpawnerIndex = 0;
    public bool _isAttacking { get; private set; } = false; //uesd to control weapon no rotation while melee attacking
    public GameObject _currentWeapon { get; private set; }

    public static event System.Action<GameObject> OnWeaponSpawned;

    void Start()
    {
        _equippedWeapons = new GameObject[2] { _weapons[_equippedMeleeIndex], _weapons[_equippedRangeIndex] };
        SpawnWeapon(null);
    }

    public void ChangeWeapon(int index)
    {
        _weaponSpawnerIndex = index - 1;
        if (_weaponSpawnerIndex < 0 || _weaponSpawnerIndex >= _equippedWeapons.Length 
            || _equippedWeapons[_weaponSpawnerIndex] == null) return;//未装备武器则不切换

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

    }

    void Update()
    {
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
