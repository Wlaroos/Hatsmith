using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyPathfinding : MonoBehaviour 
{
    public Transform _target;
    public float _speed = 5f;
    
    private Pathfinding2D _pathfinder;
    private List<Vector3> _path = new List<Vector3>();
    private int _targetIndex;

    void Start() 
    {
        _pathfinder = FindFirstObjectByType<Pathfinding2D>();
        StartCoroutine(UpdatePath());
    }

    IEnumerator UpdatePath() 
    {
        while (_target != null) 
        {
            _path = _pathfinder.FindPath(transform.position, _target.position);
            _targetIndex = 0;
            yield return new WaitForSeconds(0.25f); // Recalculate 4x per second
        }
    }

    void Update() 
    {
        if (_path == null || _path.Count == 0 || _targetIndex >= _path.Count) return;

        Vector3 currentWaypoint = _path[_targetIndex];
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentWaypoint) < 0.1f) 
        {
            _targetIndex++;
        }
    }
}