using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HatPickup : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private HatData hatData;

    [Header("Settings")]
    [SerializeField] private bool destroyOnPickup = true;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Try getting HatStackManager directly from collider or root object
        HatStackManager stackManager = other.GetComponent<HatStackManager>();
        if (stackManager == null)
        {
            stackManager = other.GetComponentInParent<HatStackManager>();
        }

        if (stackManager != null)
        {
            if (hatData != null)
            {
                // Disable collider immediately to prevent double-pickup in the same frame
                GetComponent<Collider2D>().enabled = false;
                
                stackManager.EquipHat(hatData);
            }
            else
            {
                Debug.LogWarning($"[HatPickup] No HatData assigned to {gameObject.name}!");
                return;
            }

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}