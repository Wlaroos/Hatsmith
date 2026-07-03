using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = System.Diagnostics.Debug;

public class PlayerMovement : MonoBehaviour
{

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private Sprite[] _sprites;

    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Animator _anim;

    private Vector2 _moveDirection;
    
    void Awake()
    {
        // Assigning Refs
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
    }
    
    void Update()
    {
        // Getting Movement From Inputs
        _moveDirection.x = Input.GetAxisRaw("Horizontal");
        _moveDirection.y = Input.GetAxisRaw("Vertical");
    }

    private void FixedUpdate()
    {
        // Changing Direction Sprites
        if (_moveDirection != Vector2.zero)
        {
            // switch (_moveDirection.x)
            // {
            //     case < 0:
            //         _sr.sprite = _moveDirection.y > 0 ? _sprites[0] : _sprites[3];
            //         break;
            //     case > 0:
            //         _sr.sprite = _moveDirection.y > 0 ? _sprites[2] : _sprites[5];
            //         break;
            //     default:
            //         _sr.sprite = _moveDirection.y > 0 ? _sprites[1] : _sprites[4];
            //         break;
            // }
        }

        float mouseScreenX = Input.mousePosition.x;
        _sr.flipX = mouseScreenX <= Screen.width * 0.5f;

        // Actual Movement
        _rb.MovePosition(_rb.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime);

        _anim.SetBool("IsMoving", _moveDirection != Vector2.zero);
    }
}
