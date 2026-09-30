using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class IngredientPickup : MonoBehaviour
{
    private SpriteRenderer _sr;
    private IngredientData _data;
    private float _frameRate = 0.2f;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    public void Initialize(IngredientData data, float frameRate = 0.2f)
    {
        _data = data;
        _frameRate = frameRate;
        UpdateSprite();
        StartCoroutine(AnimateSprite());
    }

    private void UpdateSprite(int frameIndex = 0)
    {
        if (_data == null) return;
        _sr.sprite = _data.frames[frameIndex];
    }

    private IEnumerator AnimateSprite()
    {
        WaitForSeconds wait = new WaitForSeconds(_frameRate);

        int frameCount = _data.frames.Length;
        int currentFrame = 0;

        while (true)
        {
            yield return wait;
            currentFrame = (currentFrame + 1) % frameCount;
            if(currentFrame < 0 || currentFrame >= frameCount) currentFrame = 0; // Safety check
            UpdateSprite(currentFrame);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerStats>(out _) && _data != null)
        {
            // Add item to manager inventory
            CraftingManager.Instance.AddIngredient(_data, 1);
            GameManager.Instance.InvokeIngredientPickedUpEvent();

            // Spawn particle effect using SO color and prefab
            if (_data.pickupEffectPrefab != null)
            {
                GameObject effect = Instantiate(_data.pickupEffectPrefab, transform.position, Quaternion.identity);
                if (effect.TryGetComponent<ParticleSystem>(out var particleSystem))
                {
                    var main = particleSystem.main;
                    main.startColor = _data.color;
                }
            }

            Destroy(gameObject);
        }
    }
}