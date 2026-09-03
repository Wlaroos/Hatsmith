using System.Collections.Generic;
using UnityEngine;

public class FlockManager : MonoBehaviour
{
    [Header("Flocking Rules")]
    [SerializeField] private float _flockRadius = 2f;
    [SerializeField] private float _flockInfluence = 0.6f;
    [SerializeField] private float _alignmentWeight = 0.5f;
    [SerializeField] private float _cohesionWeight = 0.25f;
    [SerializeField] private float _separationWeight = 0.75f;
    [SerializeField] private float _lerpSpeed = 5f;

    private static readonly List<FlockAgent> _agents = new();

    public static void RegisterAgent(FlockAgent agent)
    {
        if (!_agents.Contains(agent))
        {
            _agents.Add(agent);
        }
    }

    public static void UnregisterAgent(FlockAgent agent)
    {
        _agents.Remove(agent);
    }

    private void FixedUpdate()
    {
        int count = _agents.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            FlockAgent current = _agents[i];
            if (current == null || !current.gameObject.activeSelf) continue;

            Vector2 alignment = Vector2.zero;
            Vector2 cohesion = Vector2.zero;
            Vector2 separation = Vector2.zero;
            int neighbors = 0;

            for (int j = 0; j < count; j++)
            {
                if (i == j) continue;
                FlockAgent neighbor = _agents[j];
                if (neighbor == null || !neighbor.gameObject.activeSelf) continue;

                Vector2 offset = neighbor.Position - current.Position;
                float distance = offset.magnitude;

                if (distance > 0f && distance <= _flockRadius)
                {
                    neighbors++;
                    alignment += neighbor.Velocity;
                    cohesion += neighbor.Position;
                    separation -= offset / (distance * distance + 0.0001f);
                }
            }

            if (neighbors > 0)
            {
                alignment /= neighbors;
                cohesion = (cohesion / neighbors) - current.Position;
                separation /= neighbors;

                Vector2 flockDirection = (alignment.normalized * _alignmentWeight) +
                                         (cohesion.normalized * _cohesionWeight) +
                                         (separation.normalized * _separationWeight);

                if (flockDirection.sqrMagnitude > 0.0001f)
                {
                    current.ApplyFlockForce(flockDirection * _flockInfluence, _lerpSpeed);
                }
            }
        }
    }
}