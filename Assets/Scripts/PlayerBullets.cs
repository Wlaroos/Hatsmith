using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullets : MonoBehaviour
{

    [SerializeField] private float _shotSpeed = 5;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _knockback = 3;
    [SerializeField] private float _size = 1;
    [SerializeField] private GameObject ps;
    private Rigidbody2D _rb;
    private BoxCollider2D _bc;
    private Animator _anim;

    bool _once;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _bc = GetComponent<BoxCollider2D>();
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        StartCoroutine(DestroyBullet(8.0f));   
    }

    public void BulletSetup(Vector3 shootDir, float angle, float shotSpeed, int damage, float knockback, float size)
    {
        _shotSpeed = shotSpeed;
        _damage = damage;
        _knockback = knockback;
        _size = size;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        transform.localScale = new Vector3(_size, _size, _size);

        transform.eulerAngles = new Vector3(0, 0, angle);

        float vel = _shotSpeed;
        rb.AddForce(shootDir * vel, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {   
        if(collision.tag == "BulletBounds")
        {
            FrozenAndTrigger();
        }
        else if (collision.GetComponent<EnemyMovement>() != null)
        {
            Vector2 knockbackDirection = _rb.linearVelocity.normalized;
            collision.GetComponent<EnemyMovement>().TakeDamage(_damage, knockbackDirection * _knockback);

            FrozenAndTrigger();
        }
    }

    public IEnumerator DestroyBullet(float delay)
    {
        
        yield return new WaitForSeconds(delay);
        
        if(ps != null)
        {
            Instantiate(ps,transform.position,Quaternion.identity);
        }
        
        //CameraShaker.Instance.ShakeOnce(2f,2f,0.2f,0.2f);
        //AudioManager.PlaySound("PoisonBullet");

        Destroy(gameObject); 
    }

    public void Destroy()
    {
        if (!_once)
        {
            Debug.Log("Destroying Bullet");
            _once = true;
            StartCoroutine(DestroyBullet(0f));
        }
    }

    private void FrozenAndTrigger()
    {
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Static;

        _bc.enabled = false;

        _anim.SetTrigger("Destroy");
    }

    /* 
            if (collision.GetComponent<Enemy>() != null && gameObject.name == "NormalBullet(Clone)")
            {
                Instantiate(ps, transform.position, Quaternion.identity);
                collision.GetComponent<Enemy>().TakeDamage(_rb.linearVelocity.normalized * _knockback, _damage);
                Destroy(gameObject);
            }

            if (collision.gameObject.layer == LayerMask.NameToLayer("Walls"))
            {
                Instantiate(ps, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
    */
}
