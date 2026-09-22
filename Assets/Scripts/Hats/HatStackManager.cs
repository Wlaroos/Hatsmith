using System;
using System.Collections.Generic;
using UnityEngine;

public class HatStackManager : MonoBehaviour
{
    [SerializeField] private GameObject _hatPrefab;
    [SerializeField] private Transform _headAnchor;
    private List<HatInstance> _equippedHats = new List<HatInstance>();

    private void Awake()
    {
        // Fallback
        if (_headAnchor == null)
        {
            _headAnchor = transform;
        }
    }

    public void EquipHat(HatData hatData)
    {
        if (hatData == null || _hatPrefab == null)
        {
            Debug.LogError($"[HatStackManager] Cannot equip hat: HatData or hatPrefab is null!");
            return;
        }

        // Calculate total vertical 2D stacking offset
        float currentHeightOffset = 0f;
        foreach (var h in _equippedHats)
        {
            if (h != null && h.Data != null)
            {
                currentHeightOffset += h.Data.stackHeightOffset;
            }
        }

        // Instantiate hat as child of the anchor
        GameObject newHatGO = Instantiate(_hatPrefab, _headAnchor);
        
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
        // Set the sprite to match the HatData
        if (newHatGO.TryGetComponent<SpriteRenderer>(out var sr) && hatData.hatSprite != null)
        {
            sr.sprite = hatData.hatSprite;
        }

        // Attach and initialize HatInstance component
        HatInstance hatInstance = newHatGO.AddComponent<HatInstance>();
        hatInstance.Initialize(this.gameObject, hatData);
        _equippedHats.Add(hatInstance);
    }

    public void UnequipTopHat()
    {
        if (_equippedHats.Count == 0) return;

        int lastIndex = _equippedHats.Count - 1;
        HatInstance topHat = _equippedHats[lastIndex];

        if (topHat != null)
        {
            topHat.Remove();
        }

        _equippedHats.RemoveAt(lastIndex);
    }
}