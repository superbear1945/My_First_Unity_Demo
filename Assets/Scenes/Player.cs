using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    InputAction _move;
    Rigidbody2D _rb2d;
    SpriteRenderer _sr;
    public float _walkSpeed = 10;
    Vector3 _mouseScreenPosition;
    Vector3 _mouseWorldPosition;
    // Start is called before the first frame update
    void Start()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _move = InputSystem.actions.FindAction("Move");
        _sr = GetComponent<SpriteRenderer>();        
    }

    // Update is called once per frame
    void Update()
    {
        
        FlipPlayer();
    }

    private void FixedUpdate()
    {
        GetMousePosition();
        Move();
    }

    private void Move()
    {
        Vector2 dirction = _move.ReadValue<Vector2>();
        _rb2d.velocity = dirction * _walkSpeed;
    }

    private void FlipPlayer()
    {
        float mouseX = _mouseWorldPosition.x;
        float characterX = transform.position.x;
        if (characterX < mouseX)//face right
            _sr.flipX = false;
        else                    //face left
            _sr.flipX = true;        
    }

    void GetMousePosition()
    {
        _mouseScreenPosition = Mouse.current.position.ReadValue();
        _mouseWorldPosition = Camera.main.ScreenToWorldPoint(_mouseScreenPosition);
        _mouseWorldPosition.z += 10;
    }
}
