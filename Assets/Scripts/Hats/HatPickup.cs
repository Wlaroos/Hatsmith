using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HatPickup : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private HatData _hatData;
    [Header("Settings")]
    [SerializeField] private bool _destroyOnPickup = true;
    
    private SpriteRenderer _sr;
    private BoxCollider2D _col;
    
    public HatData Data => _hatData;

    private void OnValidate()
    {
        // In Editor
        if (_sr == null)
        {
            _sr = GetComponent<SpriteRenderer>();
        }
        if (_col == null)
        {
            _col = GetComponent<BoxCollider2D>();
        }

        if (_sr == null || _hatData == null) return;

        // Only update if the sprite in _hatData differs from current SpriteRenderer sprite
        if (_sr.sprite != _hatData.hatSprite)
        {
#if UNITY_EDITOR
            // Unsubscribe first to ensure there aren't multiple callbacks
            UnityEditor.EditorApplication.delayCall -= UpdateSpriteInEditor;
            UnityEditor.EditorApplication.delayCall += UpdateSpriteInEditor;
#endif
        }
    }

    #if UNITY_EDITOR
    private void UpdateSpriteInEditor()
    {
        UnityEditor.EditorApplication.delayCall -= UpdateSpriteInEditor;

        if (this != null && _sr != null && _hatData != null)
        {
            _sr.sprite = _hatData.hatSprite;
            _col.size = _sr.sprite.bounds.size;
            _col.offset = _sr.sprite.bounds.center;
        }
    }
    #endif

    private void Awake()
    {
        _col = GetComponent<BoxCollider2D>();
        _sr = GetComponent<SpriteRenderer>();

        if (_col != null)
        {
            _col.isTrigger = true;
        }

        // Apply sprite at runtime
        if (_sr != null && _hatData != null)
        {
            _sr.sprite = _hatData.hatSprite;
            _col.size = _sr.sprite.bounds.size;
            _col.offset = _sr.sprite.bounds.center;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HatStackManager stackManager = other.GetComponent<HatStackManager>();

        if (stackManager != null)
        {
            if (_hatData != null )
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