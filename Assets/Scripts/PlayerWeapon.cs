using UnityEngine;
using System;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private GameObject _bulletRef;
    [SerializeField] private Transform _shootTransform;
    [SerializeField] private bool _isAuto;
    [SerializeField] private float _fireDelay;
    [SerializeField] private float _bulletSize;
    private Vector3 _gunEndPointPosition;
    private Vector3 _mousePos;
    private float _startFireTime;
    private bool allowInput = true;

    private void OnEnable()
    {
        allowInput = true;
        GameManager.Instance.PlayerDownedEvent += HideWeapon;
    }

    private void OnDisable()
    {
        GameManager.Instance.PlayerDownedEvent -= HideWeapon;
    }

    private void Update()
    {
        if (allowInput && Time.timeScale == 1)
        {
            Aim();
            ShootCheck();
        }
    }

    private void Aim()
    {
        _mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        _mousePos.z = 0f;

        Vector3 aimDir = (_mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle);

        Vector3 aimLocalScale = Vector3.one;
        if (angle > 90 || angle < -90)
        {
            aimLocalScale.y = -1f;
        }
        else
        {
            aimLocalScale.y = 1f;
        }
        transform.localScale = aimLocalScale;
    }

    private void ShootCheck()
    {
        if (_isAuto)
        {
            if (Input.GetMouseButton(0) && Time.time > _fireDelay + _startFireTime)
            {
                CameraShake.Instance.Shake(0.05f, 0.05f);
                Shoot();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0) && Time.time > _startFireTime + _fireDelay)
            {
                CameraShake.Instance.Shake(0.05f, 0.05f);
                Shoot();
            } 
        }
    }

    private void Shoot()
    {
        //AudioManager.PlaySound("PoisonShot");
        
        _gunEndPointPosition = _shootTransform.position;

        Transform bulletTransform = Instantiate(_bulletRef.transform, _gunEndPointPosition, Quaternion.identity);
        Vector3 shootDir = (_mousePos - _gunEndPointPosition).normalized;
        float angle = Mathf.Atan2(shootDir.y, shootDir.x) * Mathf.Rad2Deg;

        bulletTransform.GetComponent<PlayerBullets>().BulletSetup(shootDir, angle, 20, 1, 3, _bulletSize);

        _startFireTime = Time.time;
    }

    private void HideWeapon()
    {
        allowInput = false;
        gameObject.SetActive(false);
    }
}
