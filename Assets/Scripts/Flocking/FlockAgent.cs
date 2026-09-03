using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlockAgent : MonoBehaviour
{
    private Rigidbody2D _rb;

    public Vector2 Position => transform.position;
    public Vector2 Velocity => _rb != null ? _rb.linearVelocity : Vector2.zero;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        FlockManager.RegisterAgent(this);
    }

    private void OnDisable()
    {
        FlockManager.UnregisterAgent(this);
    }

    public void ApplyFlockForce(Vector2 flockVector, float lerpSpeed)
    {
        if (_rb == null || _rb.bodyType == RigidbodyType2D.Static) return;

        // Apply flock force alongside existing velocity without overriding rigidbodies completely
        _rb.linearVelocity = Vector2.Lerp(_rb.linearVelocity, _rb.linearVelocity + flockVector, lerpSpeed * Time.fixedDeltaTime);
    }
}