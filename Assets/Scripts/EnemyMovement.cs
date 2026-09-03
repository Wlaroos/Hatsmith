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

    [Header("Obstacle Avoidance Settings")]
    [SerializeField] private LayerMask _obstacleMask;
    [SerializeField] private float _avoidanceRadius = 1.5f;
    [SerializeField] private float _obstacleAvoidanceWeight = 1.5f;
    [SerializeField] private int _rayCount = 8;

    [Header("Particle Settings")]
    [SerializeField] private GameObject _hitParticles;
    [SerializeField] private GameObject _deathParticles;

    [Header("Wander Settings")]
    [SerializeField] private float _wanderRadius = 3f;
    [SerializeField] private float _wanderInterval = 1.5f;
    [SerializeField] private float _wanderSpeedMultiplier = 0.4f;

    [Header("Animation Settings")]
    [SerializeField] private float _movementThreshold = 0.05f;

    private PlayerMovement _playerMovement;
    private PlayerHealth _playerHealth;
    
    private Animator _anim;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private BoxCollider2D _bc;
    
    [SerializeField] private int _maxHealth = 2;
    private int _currentHealth;
    private float _movementLockTimer;
    private bool _movementLocked;

    // Wander variables
    private Vector2 _wanderTarget;
    private float _wanderTimer;

    private static readonly List<EnemyMovement> _activeEnemies = new();

    private void Awake()
    {
        CacheComponents();
        AutoInitialize();
    }

    private void Start()
    {
        // Fallback if player hasn't loaded during Awake
        if (_playerMovement == null)
        {
            GetPlayerRefs();
        }

        // Initialize state if spawned outside an object pool manager
        if (_currentHealth <= 0)
        {
            _currentHealth = _maxHealth;
            _wanderTarget = transform.position;
        }
    }

    private void OnEnable()
    {
        if (!_activeEnemies.Contains(this))
        {
            _activeEnemies.Add(this);
        }

        AutoInitialize();
    }

    private void OnDisable()
    {
        _activeEnemies.Remove(this);
    }

    private void AutoInitialize()
    {
        GetPlayerRefs();
        CacheComponents();

        if (_currentHealth <= 0)
        {
            _currentHealth = _maxHealth;
        }

        if (_wanderTarget == Vector2.zero)
        {
            _wanderTarget = transform.position;
        }
    }

    public void Initialize(PlayerMovement playerMovement, PlayerHealth playerHealth)
    {
        _playerMovement = playerMovement;
        _playerHealth = playerHealth;

        GetPlayerRefs();
        CacheComponents();
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
        _wanderTimer = 0f;

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
        UpdateAnimationState();
    }

    public void Despawn()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        if (_playerMovement == null)
        {
            GetPlayerRefs();
            if (_playerMovement == null)
            {
                // Lock animation to false if no player is found
                SetIsMovingAnimation(false);
                return;
            }
        }

        if (_movementLocked)
        {
            _movementLockTimer -= Time.deltaTime;
            if (_movementLockTimer <= 0f)
            {
                _movementLocked = false;
            }
            
            // While locked/knocked back, disable walking animation
            SetIsMovingAnimation(false);
            return;
        }

        Move();
        UpdateAnimationState();
    }

    private void Move()
    {
        if (_rb == null || _rb.bodyType == RigidbodyType2D.Static)
        {
            return;
        }

        // 1. Calculate Base Target Velocity (Chase or Wander)
        Vector2 targetVelocity = ShouldWander() ? GetWanderVelocity() : GetChaseVelocity();

        // 2. Calculate Flocking Behaviors
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

        // 3. Calculate Obstacle Avoidance Vector
        Vector2 avoidanceVector = GetObstacleAvoidanceDirection();
        if (avoidanceVector.sqrMagnitude > 0.0001f)
        {
            targetVelocity += avoidanceVector * _obstacleAvoidanceWeight;
        }

        // 4. Apply Final Velocity
        targetVelocity = Vector2.ClampMagnitude(targetVelocity, _moveSpeed);
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, targetVelocity, _lerpSpeed * Time.deltaTime);

        // 5. Update Sprite Flip
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

    private Vector2 GetObstacleAvoidanceDirection()
    {
        if (_rayCount <= 0 || _obstacleMask == 0) return Vector2.zero;

        Vector2 avoidance = Vector2.zero;
        float angleStep = 360f / _rayCount;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(_obstacleMask);
        filter.useLayerMask = true;
        filter.useTriggers = true; 

        RaycastHit2D[] hitResults = new RaycastHit2D[1];

        for (int i = 0; i < _rayCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;

            // Start raycast slightly outside enemy radius to avoid self-hitting
            Vector2 rayOrigin = (Vector2)transform.position + (dir * 0.2f);

            int hitCount = Physics2D.Raycast(rayOrigin, dir, filter, hitResults, _avoidanceRadius);

            if (hitCount > 0 && hitResults[0].collider != null)
            {
                // Ignore self
                if (hitResults[0].collider.transform == transform) continue;

                float distanceWeight = 1f - (hitResults[0].distance / _avoidanceRadius);
                avoidance -= dir * distanceWeight;
            }
        }

        return avoidance.normalized;
    }

    private void UpdateAnimationState()
    {
        if (_rb == null || _anim == null) return;

        // Check velocity against squared threshold for efficiency
        bool isMoving = _rb.linearVelocity.sqrMagnitude > (_movementThreshold * _movementThreshold);
        SetIsMovingAnimation(isMoving);
    }

    private void SetIsMovingAnimation(bool isMoving)
    {
        if (_anim != null)
        {
            _anim.SetBool("IsMoving", isMoving);
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
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * _wanderRadius;
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
        
        // Ensure move animation halts during hit response
        SetIsMovingAnimation(false);
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
        if (gameObject.activeSelf)
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizes obstacle avoidance rays in the Editor when selecting the enemy
        Gizmos.color = Color.red;
        float angleStep = 360f / Mathf.Max(1, _rayCount);

        for (int i = 0; i < _rayCount; i++)
        {
            float angle = i * angleStep;
            Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + dir * _avoidanceRadius);
        }
    }
}