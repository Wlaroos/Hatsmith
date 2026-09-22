using UnityEngine;

public enum BurstOrigin
{
    HatLocation,
    EnemyLocation,
    PlayerLocation
}

[System.Serializable]
public class RetaliateBulletEffect : HatEffect
{
    public GameObject bulletPrefab;
    public BulletData bulletData;
    public int bulletCount = 8;
    public BurstOrigin spawnOrigin = BurstOrigin.HatLocation;

    public override void Execute(HatInstance hat, GameObject target = null)
    {
        if (bulletPrefab == null) return;

        // Determine spawn point based on selected enum
        Vector3 spawnPosition = GetSpawnPosition(hat, target);

        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = i * angleStep;
            Vector3 dir = Quaternion.Euler(0, 0, angle) * Vector3.right;
            
            GameObject bullet = Object.Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
            if (bullet.TryGetComponent<BulletProjectile>(out var bulletScript))
            {
                bulletScript.BulletSetup(bulletData, dir, angle);
            }
        }
    }

    private Vector3 GetSpawnPosition(HatInstance hat, GameObject target)
    {
        switch (spawnOrigin)
        {
            case BurstOrigin.EnemyLocation:
                if (target != null) return target.transform.position;
                break;

            case BurstOrigin.PlayerLocation:
                if (hat != null && hat.transform.parent != null) 
                    return hat.transform.parent.position;
                
                // Fallback attempt to find player tag if parent is missing
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) return player.transform.position;
                break;

            case BurstOrigin.HatLocation:
            default:
                if (hat != null) return hat.transform.position;
                break;
        }

        // Safety fallback if references are null
        return hat != null ? hat.transform.position : Vector3.zero;
    }
}