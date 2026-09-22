using UnityEngine;

[CreateAssetMenu(fileName = "NewBulletData", menuName = "Bullet Data")]
public class BulletData : ScriptableObject
{
    [Header("Visuals & Animations")]
    public Sprite bulletSprite;
    public RuntimeAnimatorController animatorController;

    [Header("Stats")]
    public float shotSpeed = 20f;
    public int baseDamage = 1;
    public float knockback = 3f;
    public float baseSize = 1f;
    public float lifetime = 0f;

    [Header("Particles")]
    public GameObject destroyParticlePrefab;
}