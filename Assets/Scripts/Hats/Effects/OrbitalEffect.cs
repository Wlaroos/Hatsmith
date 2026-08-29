using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class OrbitalEffect : HatEffect
{
    public GameObject orbitalPrefab;
    public float radius = 1.5f;
    public float speed = 180f;
    public int numberOfOrbitals = 3;

    [Tooltip("Maximum total orbitals allowed on this radius ring at any time.")]
    public int maxOrbitalsPerRing = 8;

    [Tooltip("Check this if firing/ticking should continuously ADD new orbitals instead of keeping a fixed count.")]
    public bool isAdditive = false;

    public override void Execute(HatInstance hat, GameObject target = null)
    {
        if (orbitalPrefab == null) return;

        Transform playerTransform = hat != null && hat.Wearer != null ? hat.Wearer.transform : target != null ? target.transform : null;
        
        if (playerTransform == null) return;

        if (!playerTransform.TryGetComponent<OrbitalTracker>(out var tracker))
        {
            tracker = playerTransform.gameObject.AddComponent<OrbitalTracker>();
        }

        tracker.SyncOrbitals(this, orbitalPrefab, numberOfOrbitals, radius, speed, isAdditive, maxOrbitalsPerRing);
    }
}

public class OrbitalTracker : MonoBehaviour
{
    private class RingData
    {
        public float Speed;
        public float CurrentAngle;
        public HashSet<OrbitalEffect> ProcessedEffects = new HashSet<OrbitalEffect>();
        public List<GameObject> Orbitals = new List<GameObject>();
    }

    private Dictionary<float, RingData> rings = new Dictionary<float, RingData>();

    public void SyncOrbitals(OrbitalEffect effect, GameObject prefab, int count, float radius, float speed, bool isAdditive, int maxOrbitals)
    {
        float radiusKey = Mathf.Round(radius * 100f) / 100f;

        if (!rings.ContainsKey(radiusKey))
        {
            rings[radiusKey] = new RingData();
        }

        RingData ring = rings[radiusKey];
        ring.Speed = speed;

        // Clean null references
        ring.Orbitals.RemoveAll(item => item == null);

        // Only spawn if additive OR if this effect hasn't spawned its initial batch yet
        if (isAdditive || !ring.ProcessedEffects.Contains(effect))
        {
            ring.ProcessedEffects.Add(effect);

            for (int i = 0; i < count; i++)
            {
                // Stop spawning if we reached the maximum limit for this ring
                if (ring.Orbitals.Count >= maxOrbitals)
                {
                    break;
                }

                GameObject orbitalGO = Instantiate(prefab, transform);
                ring.Orbitals.Add(orbitalGO);
            }

            // Immediately re-space remaining and new orbitals evenly
            UpdateRingPositions(radiusKey, ring);
        }
    }

    private void Update()
    {
        foreach (var kvp in rings)
        {
            float radius = kvp.Key;
            RingData ring = kvp.Value;

            // Remove destroyed projectiles and update spacing
            if (ring.Orbitals.RemoveAll(item => item == null) > 0)
            {
                UpdateRingPositions(radius, ring);
            }

            if (ring.Orbitals.Count == 0) continue;

            ring.CurrentAngle += ring.Speed * Time.deltaTime;
            if (ring.CurrentAngle >= 360f) ring.CurrentAngle -= 360f;

            UpdateRingPositions(radius, ring);
        }
    }

    private void UpdateRingPositions(float radius, RingData ring)
    {
        ring.Orbitals.RemoveAll(item => item == null);

        int count = ring.Orbitals.Count;
        if (count == 0) return;

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float orbitalAngle = ring.CurrentAngle + (i * angleStep);
            float radians = orbitalAngle * Mathf.Deg2Rad;

            float x = Mathf.Cos(radians) * radius;
            float y = Mathf.Sin(radians) * radius;

            ring.Orbitals[i].transform.localPosition = new Vector3(x, y, 0f);
            ring.Orbitals[i].transform.rotation = Quaternion.identity;
        }
    }

    private void OnDestroy()
    {
        foreach (var ring in rings.Values)
        {
            foreach (var orb in ring.Orbitals)
            {
                if (orb != null) Destroy(orb);
            }
            ring.Orbitals.Clear();
            ring.ProcessedEffects.Clear();
        }
        rings.Clear();
    }
}