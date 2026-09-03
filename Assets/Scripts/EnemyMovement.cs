using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _lerpSpeed = 5f;
    [SerializeField] private float _nextWaypointDistance = 0.2f;
    [SerializeField] private float _pathUpdateInterval = 0.25f;

    [Header("Knockback Settings")]
    [SerializeField] private float _movementLockDuration = 0.1f;

    [Header("Particle Settings")]
    [SerializeField] private GameObject _hitParticles;

    [Header("Wander Settings")]
    [SerializeField] private float _wanderRadius = 3f;
    [SerializeField] private float _wanderInterval = 1.5f;
    [SerializeField] private float _wanderSpeedMultiplier = 0.4f;

    [Header("Animation & Stats")]
    [SerializeField] private float _movementThreshold = 0.05f;
    [SerializeField] private int _maxHealth = 2;

    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;
    private Pathfinding2D _pathfinder;
    
    private Animator _anim;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private BoxCollider2D _bc;
    
    private int _currentHealth;
    private float _movementLockTimer;
    private bool _movementLocked;

    private List<Vector3> _path;
    private int _targetWaypointIndex;

    private Vector2 _wanderTarget;
    private float _wanderTimer;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _bc = GetComponent<BoxCollider2D>();
        _pathfinder = FindFirstObjectByType<Pathfinding2D>();
        
        GetPlayerRefs();
        _currentHealth = _maxHealth;
        _wanderTarget = transform.position;
    }

    private void OnEnable()
    {
        StartCoroutine(UpdatePathRoutine());
    }

    public void Spawn(Vector2 position)
    {
        transform.position = position;
        _wanderTarget = position;
        _wanderTimer = 0f;
        _currentHealth = _maxHealth;
        _movementLocked = false;

        if (_bc != null) _bc.enabled = true;
        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.linearVelocity = Vector2.zero;
        }

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (_playerMovement == null && !GetPlayerRefs())
        {
            SetIsMovingAnimation(false);
            return;
        }

        if (_movementLocked)
        {
            _movementLockTimer -= Time.deltaTime;
            if (_movementLockTimer <= 0f) _movementLocked = false;
            
            SetIsMovingAnimation(false);
            return;
        }

        Move();
        UpdateAnimationState();
    }

    private void Move()
    {
        if (_rb == null || _rb.bodyType == RigidbodyType2D.Static) return;

        Vector2 targetVelocity = ShouldWander() ? GetWanderVelocity() : GetAStarVelocity();

        targetVelocity = Vector2.ClampMagnitude(targetVelocity, _moveSpeed);
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVelocity, _lerpSpeed * Time.deltaTime);

        if (_sr != null)
        {
            Vector2 facingDirection = _rb.linearVelocity.sqrMagnitude > 0.01f 
                ? _rb.linearVelocity 
                : (ShouldWander() ? (_wanderTarget - (Vector2)transform.position) : (_playerMovement.transform.position - transform.position));

            if (Mathf.Abs(facingDirection.x) > 0.01f)
            {
                _sr.flipX = facingDirection.x < 0;
            }
        }
    }

    private Vector2 GetAStarVelocity()
    {
        if (_path == null || _path.Count == 0 || _targetWaypointIndex >= _path.Count)
            return Vector2.zero;

        Vector3 targetWaypoint = _path[_targetWaypointIndex];
        Vector2 offset = targetWaypoint - transform.position;

        if (offset.sqrMagnitude <= _nextWaypointDistance * _nextWaypointDistance)
        {
            _targetWaypointIndex++;
            if (_targetWaypointIndex >= _path.Count) return Vector2.zero;
            offset = _path[_targetWaypointIndex] - transform.position;
        }

        return offset.normalized * _moveSpeed;
    }

    private IEnumerator UpdatePathRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(_pathUpdateInterval);
        while (true)
        {
            if (_pathfinder != null && _playerMovement != null && !ShouldWander() && !_movementLocked)
            {
                _path = _pathfinder.FindPath(transform.position, _playerMovement.transform.position);
                _targetWaypointIndex = 0;
            }
            yield return wait;
        }
    }

    public void TakeDamage(int damage, Vector2 knockbackForce)
    {
        _currentHealth -= damage;
        _movementLocked = true;
        _movementLockTimer = _movementLockDuration;
        
        SetIsMovingAnimation(false);
        Knockback(knockbackForce);

        if (_hitParticles != null)
        {
            float angle = knockbackForce.sqrMagnitude > 0f ? Mathf.Atan2(knockbackForce.y, knockbackForce.x) * Mathf.Rad2Deg : 0f;
            Instantiate(_hitParticles, transform.position, Quaternion.Euler(0f, 0f, angle));
        }

        if (_currentHealth <= 0)
        {
            if (GameManager.Instance != null) GameManager.Instance.InvokeEnemyKilledEvent(gameObject);

            _rb.linearVelocity = Vector2.zero;
            _rb.bodyType = RigidbodyType2D.Static;
            if (_bc != null) _bc.enabled = false;

            if (_anim != null)
            {
                _anim.SetBool("IsMoving", false);
                _anim.SetTrigger("Destroy");
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private bool GetPlayerRefs()
    {
        _playerMovement ??= FindFirstObjectByType<PlayerMovement>();
        if (_playerMovement != null)
        {
            _playerHealth ??= _playerMovement.GetComponent<PlayerHealth>();
        }
        return _playerMovement != null;
    }

    private bool ShouldWander() => _playerHealth != null && _playerHealth.IsDowned;

    private Vector2 GetWanderVelocity()
    {
        _wanderTimer -= Time.deltaTime;
        Vector2 offset = _wanderTarget - (Vector2)transform.position;

        if (_wanderTimer <= 0f || offset.sqrMagnitude <= 0.04f)
        {
            _wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * _wanderRadius;
            _wanderTimer = Random.Range(_wanderInterval * 0.5f, _wanderInterval * 1.5f);
            offset = _wanderTarget - (Vector2)transform.position;
        }

        return offset.normalized * (_moveSpeed * _wanderSpeedMultiplier);
    }

    private void UpdateAnimationState()
    {
        if (_rb != null && _anim != null)
        {
            SetIsMovingAnimation(_rb.linearVelocity.sqrMagnitude > (_movementThreshold * _movementThreshold));
        }
    }

    private void SetIsMovingAnimation(bool isMoving)
    {
        if (_anim != null) _anim.SetBool("IsMoving", isMoving);
    }

    private void Knockback(Vector2 force)
    {
        if (_rb != null && _rb.bodyType != RigidbodyType2D.Static)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    public void Destroy() 
    {
        // Deactivate to return back to EnemyManager pool instead of destroying
        gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        if (_path == null || _path.Count == 0) return;

        Gizmos.color = Color.green;
        for (int i = _targetWaypointIndex; i < _path.Count; i++)
        {
            Gizmos.DrawCube(_path[i], Vector3.one * 0.15f);
            Gizmos.DrawLine(i == _targetWaypointIndex ? transform.position : _path[i - 1], _path[i]);
        }
    }
}