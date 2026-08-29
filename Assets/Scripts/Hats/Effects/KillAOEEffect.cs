using UnityEngine;

[System.Serializable]
public class KillAOEEffect : HatEffect
{
    public float radius = 3f;
    public int damage = 2;
    public float knockback = 5f;
    public GameObject explosionFX;

    public override void Execute(HatInstance hat, GameObject target = null)
    {
        Vector3 origin = target != null ? target.transform.position : hat.transform.position;
        
        if (explosionFX != null)
        {
            Object.Instantiate(explosionFX, origin, Quaternion.identity);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyMovement>(out var enemy))
            {
                Vector2 dir = (enemy.transform.position - origin).normalized;
                enemy.TakeDamage(damage, dir * knockback);
            }
        }
    }
}
