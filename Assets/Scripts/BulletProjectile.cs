using System.Collections;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    private float _shotSpeed;
    private int _damage;
    private float _knockback;
    private float _size;
    private float _lifetime;
    private GameObject _destroyParticlePrefab;

    private Rigidbody2D _rb;
    private BoxCollider2D _bc;
    private Animator _anim;
    private SpriteRenderer _spriteRenderer;

    private bool _once;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _bc = GetComponent<BoxCollider2D>();
        _anim = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Changes bullet params using ScriptableObject data.
    public void BulletSetup(BulletData data, Vector3 shootDir, float angle, float playerSizeMultiplier = 1f, int playerDamageBonus = 0)
    {
        if (data == null) return;

        // Apply Data Values
        _shotSpeed = data.shotSpeed;
        _damage = data.baseDamage + playerDamageBonus;
        _knockback = data.knockback;
        _size = data.baseSize * playerSizeMultiplier;
        _lifetime = data.lifetime;
        _destroyParticlePrefab = data.destroyParticlePrefab;

        // Apply Visuals & Animations
        if (_spriteRenderer != null && data.bulletSprite != null)
        {
            _spriteRenderer.sprite = data.bulletSprite;
        }

        if (_anim != null && data.animatorController != null)
        {
            _anim.runtimeAnimatorController = data.animatorController;
        }

        // Apply Transform & Physics Setup
        transform.localScale = new Vector3(_size, _size, _size);
        transform.eulerAngles = new Vector3(0, 0, angle);

        _rb.AddForce(shootDir * _shotSpeed, ForceMode2D.Impulse);

        if (_lifetime > 0f)
        {
            StartCoroutine(DestroyBullet(_lifetime));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if (collision.CompareTag("BulletBounds"))
        {
            FrozenAndTrigger();
        }
        else if (collision.TryGetComponent<EnemyMovement>(out var enemy))
        {
            Vector2 knockbackDirection = _rb.linearVelocity.normalized;
            GameManager.Instance?.InvokeEnemyHitEvent(enemy.gameObject);
            enemy.TakeDamage(_damage, knockbackDirection * _knockback);

            FrozenAndTrigger();
        }
    }

    public IEnumerator DestroyBullet(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject); 
    }

    public void Destroy()
    {
        if (!_once)
        {
            _once = true;
            StartCoroutine(DestroyBullet(0f));
        }
    }

    private void FrozenAndTrigger()
    {
        if (_rb.bodyType != RigidbodyType2D.Static) 
            _rb.linearVelocity = Vector2.zero;

        _rb.bodyType = RigidbodyType2D.Static;
        _bc.enabled = false;

        if (_anim != null && _anim.runtimeAnimatorController != null)
        {
            _anim.SetTrigger("Destroy");
        }

        if (_destroyParticlePrefab != null)
        {
            Instantiate(_destroyParticlePrefab, transform.position, Quaternion.identity);
        }
    }
}