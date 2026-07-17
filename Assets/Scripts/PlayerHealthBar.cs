using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject _iconPrefab;
    [SerializeField] private Sprite[] _statusSprites; // 0 = heart, 1 = revive, 2 = empty
    private PlayerHealth _playerHealth;
    private List<GameObject> _heartIcons = new();

    private void Awake()
    {
        _playerHealth = FindAnyObjectByType<PlayerHealth>();

        for (int i = 0; i < _playerHealth.MaxHealth; i++)
        {
            var icon = Instantiate(_iconPrefab, transform);
            _heartIcons.Add(icon);
        }
    }

    private void Start()
    {
        UpdateHealthBar();
    }

    private void OnEnable()
    {
        _playerHealth.DamageEvent.AddListener(UpdateHealthBar);
    }

    private void OnDisable()
    {
        _playerHealth.DamageEvent.RemoveListener(UpdateHealthBar);
    }

    private void UpdateHealthBar()
    {
        for (int i = 0; i < _heartIcons.Count; i++)
        {
            var img = _heartIcons[i].GetComponent<Image>();
            _heartIcons[i].SetActive(true);

            if (i < _playerHealth.CurrentHealth)
            {
                img.sprite = _statusSprites[0];
            }
            else
            {
                _heartIcons[i].SetActive(false);
            }
        }
    }

}
