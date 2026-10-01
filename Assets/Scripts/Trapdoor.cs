using UnityEngine;

public class Trapdoor : MonoBehaviour
{
    [SerializeField] private Sprite _openSprite;
    [SerializeField] private GameObject _particlePrefab;
    private SpriteRenderer _sr;
    private BoxCollider2D _bc;
    private bool _open = false;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _bc = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        GameManager.Instance.RoomEnemiesKilledEvent += OnRoomClear;
    }

    private void OnDisable()
    {
        GameManager.Instance.RoomEnemiesKilledEvent -= OnRoomClear;
    }

    private void OnRoomClear()
    {
        _sr.sprite = _openSprite;
        _open = true;
        
        if (_particlePrefab != null)
        {
            Instantiate(_particlePrefab, transform.position, Quaternion.identity);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && _open)
        {
            LevelGenerator2D.Instance.SpawnRandomRoom();
        }
    }
}
