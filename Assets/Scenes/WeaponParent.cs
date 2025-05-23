using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Weapon : MonoBehaviour
{
    Vector3 _mouseScreenPosition;
    Vector3 _mouseWorldPosition;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        GetMousePosition();
        AimToMouse();
        FlipWeapon();
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

    void FlipWeapon()
    {
        SpriteRenderer weaponRender = GetComponentInChildren<SpriteRenderer>();
        Debug.Log(weaponRender.sprite);
    }
}
