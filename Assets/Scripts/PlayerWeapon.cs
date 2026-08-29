using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject _bulletRef;
    [SerializeField] private Transform _shootTransform;
    [SerializeField] private bool _isAuto;

    private Vector3 _gunEndPointPosition;
    private Vector3 _mousePos;
    private float _startFireTime;
    private PlayerStats _stats;
    private Camera _mainCamera;
    private bool _allowInput = true;

    private void Awake()
    {
        _stats = GetComponentInParent<PlayerStats>();
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        _allowInput = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDownedEvent += HideWeapon;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDownedEvent -= HideWeapon;
        }
    }

    private void Update()
    {
        if (_allowInput && Time.timeScale == 1)
        {
            Aim();
            ShootCheck();
        }
    }

    private void Aim()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;

        _mousePos = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0f;

        Vector3 aimDir = (_mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 aimLocalScale = Vector3.one;
        aimLocalScale.y = (angle > 90 || angle < -90) ? -1f : 1f;
        transform.localScale = aimLocalScale;
    }

    private void ShootCheck()
    {
        float fireRate = _stats != null ? _stats.FireDelay : 0.5f;

        if (_isAuto)
        {
            if (Input.GetMouseButton(0) && Time.time > fireRate + _startFireTime)
            {
                CameraShake.Instance?.Shake(0.05f, 0.05f);
                Shoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && Time.time > _startFireTime + fireRate)
            {
                CameraShake.Instance?.Shake(0.05f, 0.05f);
                Shoot();
            } 
        }
    }

    private void Shoot()
    {
        _gunEndPointPosition = _shootTransform.position;

        Transform bulletTransform = Instantiate(_bulletRef.transform, _gunEndPointPosition, Quaternion.identity);
        Vector3 shootDir = (_mousePos - _gunEndPointPosition).normalized;
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;

        float bulletSize = _stats != null ? _stats.BulletSize : 1f;
        int damage = 1 + Mathf.RoundToInt(_stats != null ? _stats.GetDamageBonus() : 0f);
        bulletTransform.GetComponent<PlayerBullets>()?.BulletSetup(shootDir, angle, 20, damage, 3, bulletSize);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.InvokePlayerShootEvent(gameObject);
        }

        _startFireTime = Time.time;
    }

    private void HideWeapon()
    {
        _allowInput = false;
        gameObject.SetActive(false);
    }
}