using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HatPickup : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private HatData _hatData;
    [Header("Settings")]
    [SerializeField] private bool _destroyOnPickup = true;
    private SpriteRenderer _sr;
    public HatData Data => _hatData;

    private void OnValidate()
    {
        // Automatically sync visual sprite in the editor when HatData changes
        if (_hatData != null && _hatData.hatSprite != null)
        {
            if (_sr == null)
            {
                _sr = GetComponent<SpriteRenderer>();
            }

            if (_sr != null)
            {
                _sr.sprite = _hatData.hatSprite;
            }
            else
            {
                Debug.LogWarning($"[HatPickup] No SpriteRenderer found on {gameObject.name} to sync with HatData sprite!");
            }
        }
    }

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        _sr = GetComponent<SpriteRenderer>();

        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HatStackManager stackManager = other.GetComponent<HatStackManager>() ?? other.GetComponentInParent<HatStackManager>();

        if (stackManager != null)
        {
            if (_hatData != null)
            {
                GetComponent<Collider2D>().enabled = false;
                stackManager.EquipHat(_hatData);
            }
            else
            {
                Debug.LogWarning($"[HatPickup] No HatData assigned to {gameObject.name}!");
                return;
            }

            if (_destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}