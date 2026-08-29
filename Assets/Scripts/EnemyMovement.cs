using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _lerpSpeed = 5f;

    [Header("Knockback Settings")]
    [SerializeField] private float _movementLockDuration = 0.1f;

    [Header("Flocking Settings")]
    [SerializeField] private float _flockRadius = 2f;
    [SerializeField] private float _flockInfluence = 0.6f;
    [SerializeField] private float _alignmentWeight = 0.5f;
    [SerializeField] private float _cohesionWeight = 0.25f;
    [SerializeField] private float _separationWeight = 0.75f;

    [Header("Particle Settings")]
    [SerializeField] private GameObject _hitParticles;
    [SerializeField] private GameObject _deathParticles;

    [Header("Wander Settings")]
    [SerializeField] private float _wanderRadius = 3f;
    [SerializeField] private float _wanderInterval = 1.5f;
    [SerializeField] private float _wanderSpeedMultiplier = 0.4f;

    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;
    
    private Animator _anim;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private BoxCollider2D _bc;
    
    private int _maxHealth = 2;
    private int _currentHealth;
    private float _movementLockTimer;
    private bool _movementLocked;

    // Wander variables
    private Vector2 _wanderTarget;
    private float _wanderTimer;

    private static readonly List<EnemyMovement> _activeEnemies = new();

    public void Initialize(PlayerMovement playerMovement, PlayerHealth playerHealth)
    {
        _playerMovement = playerMovement;
        _playerHealth = playerHealth;

        GetPlayerRefs();
        CacheComponents();
    }

    private void OnEnable()
    {
        if (!_activeEnemies.Contains(this))
        {
            _activeEnemies.Add(this);
        }
    }

    private void OnDisable()
    {
        _activeEnemies.Remove(this);
    }

    private void GetPlayerRefs()
    {
        if (_playerMovement == null)
        {
            _playerMovement = FindAnyObjectByType<PlayerMovement>();
        }

        if (_playerMovement != null)
        {
            _playerHealth ??= _playerMovement.GetComponent<PlayerHealth>();
        }
    }

    private void CacheComponents()
    {
        _anim ??= GetComponent<Animator>();
        _rb ??= GetComponent<Rigidbody2D>();
        _sr ??= GetComponent<SpriteRenderer>();
        _bc ??= GetComponent<BoxCollider2D>();
    }

    public void Spawn(Vector2 position)
    {
        transform.position = position;

        GetPlayerRefs();
        CacheComponents();

        _wanderTarget = position;
        _wanderTimer = 0f; // Forces immediate wander target selection

        _currentHealth = _maxHealth;
        _movementLocked = false;
        _movementLockTimer = 0f;

        if (_bc != null) _bc.enabled = true;

        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.linearVelocity = Vector2.zero;
        }

        gameObject.SetActive(true);

        if (_anim != null)
        {
            _anim.SetBool("IsMoving", true);
        }
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (_playerMovement == null || !gameObject.activeSelf)
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

        Vector2 targetVelocity = ShouldWander() ? GetWanderVelocity() : GetChaseVelocity();

        Vector2 alignment = Vector2.zero;
        Vector2 cohesion = Vector2.zero;
        Vector2 separation = Vector2.zero;
        int nearbyCount = 0;

        foreach (EnemyMovement other in _activeEnemies)
        {
            if (other == null || other == this || !other.gameObject.activeSelf)
            {
                continue;
            }

            Vector2 offset = other.transform.position - transform.position;
            float distance = offset.magnitude;
            if (distance <= 0f || distance > _flockRadius)
            {
                continue;
            }

            nearbyCount++;
            alignment += other._rb != null ? other._rb.linearVelocity : Vector2.zero;
            cohesion += (Vector2)other.transform.position;
            separation -= offset / (distance * distance + 0.0001f);
        }

        if (nearbyCount > 0)
        {
            alignment /= nearbyCount;
            cohesion = (cohesion / nearbyCount) - (Vector2)transform.position;
            separation /= nearbyCount;

            Vector2 flockDirection = (alignment.normalized * _alignmentWeight) + 
                                     (cohesion.normalized * _cohesionWeight) + 
                                     (separation.normalized * _separationWeight);

            if (flockDirection.sqrMagnitude > 0.0001f)
            {
                targetVelocity += flockDirection * _flockInfluence;
            }
        }

        targetVelocity = Vector2.ClampMagnitude(targetVelocity, _moveSpeed);
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVelocity, _lerpSpeed * Time.deltaTime);

        if (_sr != null)
        {
            Vector2 facingDirection = ShouldWander() ? (_wanderTarget - (Vector2)transform.position) : (_playerMovement.transform.position - transform.position);
            _sr.flipX = facingDirection.x < 0;
        }
    }

    private bool ShouldWander()
    {
        return _playerHealth != null && _playerHealth.IsDowned;
    }

    private Vector2 GetChaseVelocity()
    {
        if (_playerMovement == null)
        {
            return Vector2.zero;
        }

        return (_playerMovement.transform.position - transform.position).normalized * _moveSpeed;
    }

    private Vector2 GetWanderVelocity()
    {
        _wanderTimer -= Time.deltaTime;
        Vector2 offset = _wanderTarget - (Vector2)transform.position;

        if (_wanderTimer <= 0f || offset.sqrMagnitude <= 0.04f)
        {
            // Generate the random point
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * _wanderRadius;
            
            // Clamp it to the screen bounds before setting it as the target
            _wanderTarget = ClampToScreenBounds(randomPoint);
            
            _wanderTimer = Random.Range(_wanderInterval * 0.5f, _wanderInterval * 1.5f);
            offset = _wanderTarget - (Vector2)transform.position;
        }

        return offset.normalized * (_moveSpeed * _wanderSpeedMultiplier);
    }

    private Vector2 ClampToScreenBounds(Vector2 targetPosition)
    {
        Camera cam = Camera.main;
        if (cam == null) return targetPosition;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // Subtract a margin for the walls (Size 1 for a 1x1 square)
        float margin = 1f; 
        float clampX = Mathf.Clamp(targetPosition.x, -halfWidth + margin, halfWidth - margin);
        float clampY = Mathf.Clamp(targetPosition.y, -halfHeight + margin, halfHeight - margin);

        return new Vector2(clampX, clampY);
    }

    public void TakeDamage(int damage, Vector2 knockbackForce)
    {
        _currentHealth -= damage;
        _movementLocked = true;
        _movementLockTimer = _movementLockDuration;
        Knockback(knockbackForce);

        if (_hitParticles != null)
        {
            Quaternion particleRotation = Quaternion.identity;
            if (knockbackForce.sqrMagnitude > 0f)
            {
                float angle = Mathf.Atan2(knockbackForce.y, knockbackForce.x) * Mathf.Rad2Deg;
                particleRotation = Quaternion.Euler(0f, 0f, angle);
            }

            Instantiate(_hitParticles, transform.position, particleRotation);
        }

        if (_currentHealth <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.InvokeEnemyKilledEvent(gameObject);
            }

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
            _rb.linearVelocity = Vector2.zero;
            _rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    public void Destroy()
    {
        Despawn();
        _currentHealth = _maxHealth;
    }
}