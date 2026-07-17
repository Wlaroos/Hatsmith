using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _maxHealth = 5;
    public int MaxHealth => _maxHealth;
    [SerializeField] private float _invincibilityDuration = 1f;

    [Header("Flash")]
    [SerializeField] private float _flashInterval = 0.08f;
    [SerializeField] private Color _flashHitColor = new Color(1f, 0f, 0f, 0.4f);
    [SerializeField] private Color _normalColor = Color.white;

    private int _currentHealth;
    public int CurrentHealth => _currentHealth;
    private int _currentRevives;
    public int CurrentRevives => _currentRevives;
    private bool _isInvincible;
    private bool _isDowned;
    public bool IsDowned => _isDowned;
    private SpriteRenderer _sr;
    private Animator _anim;

    public UnityEvent DamageEvent = new UnityEvent();

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();

        _currentHealth = _maxHealth;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDowned || _isInvincible)
            return;

        if (collision.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (_isDowned || _isInvincible)
            return;

        _currentHealth -= damage;

        DamageEvent.Invoke();

        if (_currentHealth <= 0)
        {
            Down();
            return;
        }

        StartCoroutine(InvincibilityFlash());
    }

    private IEnumerator InvincibilityFlash()
    {
        _isInvincible = true;
        float timer = 0f;
        bool showHitColor = false;

        while (timer < _invincibilityDuration)
        {
            showHitColor = !showHitColor;
            _sr.color = showHitColor ? _flashHitColor : _normalColor;

            timer += _flashInterval;
            yield return new WaitForSeconds(_flashInterval);
        }

        _sr.color = _normalColor;
        _isInvincible = false;
    }

    private void Down()
    {
        _isDowned = true;
        _sr.color = _normalColor;

        var movement = GetComponent<PlayerMovement>();
        if (movement != null)
            movement.enabled = false;

        var collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        _anim.SetTrigger("Down");

        Debug.Log("Player downed");
    }
}
