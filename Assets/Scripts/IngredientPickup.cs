using System.Collections;
using UnityEngine;

public class IngredientPickup : MonoBehaviour
{
    public enum IngredientType
    {
        Purple,
        Green,
        Red,
        Orange,
        Blue,
        Pink
    }

    [SerializeField] private IngredientType _ingredientType;
    [SerializeField] private Sprite[] _frame01;
    [SerializeField] private Sprite[] _frame02;
    [SerializeField] private float _frameRate = 0.2f;

    private SpriteRenderer _sr;
    private bool _isFrame01 = true;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        StartCoroutine(AnimateSprite());
    }

    public void SetIngredientType(IngredientType type)
    {
        _ingredientType = type;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        int index = (int)_ingredientType;

        if (_frame01 != null && index < _frame01.Length && _frame02 != null && index < _frame02.Length)
        {
            // Set the sprite based on the current frame and ingredient type
            _sr.sprite = _isFrame01 ? _frame01[index] : _frame02[index];
        }
    }

    private IEnumerator AnimateSprite()
    {
        WaitForSeconds wait = new WaitForSeconds(_frameRate);

        while (true)
        {
            _isFrame01 = !_isFrame01;
            UpdateSprite();
            yield return wait;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerStats>(out _))
        {
            GameManager.Instance.AddIngredientCount((int)_ingredientType, 1);
            Destroy(gameObject);
        }
    }
}