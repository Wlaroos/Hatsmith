using UnityEngine;

[System.Serializable]
public class RetaliateBulletEffect : HatEffect
{
    public GameObject bulletPrefab;
    public int bulletCount = 8;

    public override void Execute(HatInstance hat, GameObject target = null)
    {
        if (bulletPrefab == null) return;

        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
            
            GameObject bullet = Object.Instantiate(bulletPrefab, hat.transform.position, Quaternion.identity);
            if (bullet.TryGetComponent<PlayerBullets>(out var bulletScript))
            {
                bulletScript.BulletSetup(dir, angle, 12f, 1, 2f, 0.8f);
            }
        }
    }
}
