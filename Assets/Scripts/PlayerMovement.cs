using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _anim;
    private PlayerStats _stats;

    private Vector2 _moveDirection;
    
    void Awake()
    {
        // Assigning Refs
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
        _stats = GetComponent<PlayerStats>();
    }
    
    void Update()
    {
        // Getting Movement From Inputs
        _moveDirection.x = Input.GetAxisRaw("Horizontal");
        _moveDirection.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        float mouseScreenX = Input.mousePosition.x;
        _sr.flipX = mouseScreenX <= Screen.width * 0.5f;

        // Actual Movement
        _rb.MovePosition(_rb.position + _moveDirection * _stats.MoveSpeed * Time.fixedDeltaTime);

        _anim.SetBool("IsMoving", _moveDirection != Vector2.zero);
    }
}
