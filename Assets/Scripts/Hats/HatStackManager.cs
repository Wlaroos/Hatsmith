using System;
using System.Collections.Generic;
using UnityEngine;

public class HatStackManager : MonoBehaviour
{
    [SerializeField] private Transform headAnchor;
    private List<HatInstance> equippedHats = new List<HatInstance>();

    private void Awake()
    {
        // Fallback
        if (headAnchor == null)
        {
            headAnchor = transform;
        }
    }

    public void EquipHat(HatData hatData)
    {
        if (hatData == null || hatData.hatPrefab == null)
        {
            Debug.LogError($"[HatStackManager] Cannot equip hat: HatData or hatPrefab is null!");
            return;
        }

        // Calculate total vertical 2D stacking offset
        float currentHeightOffset = 0f;
        foreach (var h in equippedHats)
        {
            if (h != null && h.Data != null)
            {
                currentHeightOffset += h.Data.stackHeightOffset;
            }
        }

        // Instantiate hat as child of the anchor
        GameObject newHatGO = Instantiate(hatData.hatPrefab, headAnchor);
        
        // Reset transform values for clean 2D positioning
        newHatGO.transform.localPosition = new Vector3(0f, currentHeightOffset, 0f);
        newHatGO.transform.localRotation = Quaternion.identity;
        newHatGO.transform.localScale = Vector3.one;

        // Remove HatPickup component so equipped hats don't trigger pickup logic again
        if (newHatGO.TryGetComponent<HatPickup>(out var pickup))
        {
            Destroy(pickup);
        }
        // Disable the trigger collider so projectiles/bullets don't hit the hat
        if (newHatGO.TryGetComponent<Collider2D>(out var col))
        {
            col.enabled = false;
        }

        // Attach and initialize HatInstance component
        HatInstance hatInstance = newHatGO.AddComponent<HatInstance>();
        hatInstance.Initialize(this.gameObject, hatData);
        equippedHats.Add(hatInstance);
    }

    public void UnequipTopHat()
    {
        if (equippedHats.Count == 0) return;

        int lastIndex = equippedHats.Count - 1;
        HatInstance topHat = equippedHats[lastIndex];

        if (topHat != null)
        {
            topHat.Remove();
        }

        equippedHats.RemoveAt(lastIndex);
    }
}