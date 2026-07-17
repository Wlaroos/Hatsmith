using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _lerpSpeed = 5f;
    [SerializeField] private float _movementLockDuration = 0.1f;

    private PlayerMovement _player;
    private Animator _anim;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private BoxCollider2D _bc;
    private int _health = 2;
    private float _movementLockTimer;
    private bool _movementLocked;

    public void Initialize(PlayerMovement player)
    {
        _player = player;
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _bc = GetComponent<BoxCollider2D>();
    }

    public void Spawn(Vector2 position)
    {
        transform.position = position;

        // Ensure components and player reference are valid when reused from a pool
        if (_player == null)
        {
            _player = FindAnyObjectByType<PlayerMovement>();
        }

        if (_anim == null) _anim = GetComponent<Animator>();
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_bc == null) _bc = GetComponent<BoxCollider2D>();

        // Reset state for reuse
        if (_anim != null)
        {
            _anim.SetBool("IsMoving", true);
        }

        // Reset health and movement lock state
        _health = 2;

        _movementLocked = false;
        _movementLockTimer = 0f;

        // Enable the collider and Rigidbody2D for interaction
        _bc.enabled = true;

        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.linearVelocity = Vector2.zero;
        }

        // Activate the GameObject to make it visible and interactive in the scene
        gameObject.SetActive(true);
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_player == null || !gameObject.activeSelf)
        {
            return;
        }

        if (_movementLocked)
        {
            _movementLockTimer -= Time.deltaTime;
            if (_movementLockTimer <= 0f)
            {
                _movementLocked = false;
            }
            return;
        }

        Move();
    }

    private void Move()
    {
        if (_rb == null || _rb.bodyType == RigidbodyType2D.Static)
        {
            return;
        }

        Vector2 direction = (_player.transform.position - transform.position).normalized;
        Vector2 targetVelocity = direction * _moveSpeed;
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVelocity, _lerpSpeed * Time.deltaTime);

        if (_sr != null)
        {
            _sr.flipX = direction.x < 0;
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackForce)
    {
        _health -= damage;
        _movementLocked = true;
        _movementLockTimer = _movementLockDuration;
        Knockback(knockbackForce);

        if (_health <= 0)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Static;
            _bc.enabled = false;

            _anim.SetTrigger("Destroy");
        }
    }

    private void Knockback(Vector2 force)
    {
        if (_rb != null && _rb.bodyType != RigidbodyType2D.Static)
        {
            _rb.linearVelocity = Vector2.zero; // Reset velocity before applying knockback
            _rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    public void Destroy()
    {
        Despawn();
        _health = 2; // Reset health for reuse
    }
    
}
