using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
    private SpriteRenderer _sr;
    private bool _isFrame01 = true;
    private int _repeatCount = 1000;

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
    }

    private void SetSprite(bool frame)
    {
        switch (_ingredientType)
        {
            case IngredientType.Purple:
                _sr.sprite = frame ? _frame01[(int)IngredientType.Purple] : _frame02[(int)IngredientType.Purple];
                break;
            case IngredientType.Green:
                _sr.sprite = frame ? _frame01[(int)IngredientType.Green] : _frame02[(int)IngredientType.Green];
                break;
            case IngredientType.Red:
                _sr.sprite = frame ? _frame01[(int)IngredientType.Red] : _frame02[(int)IngredientType.Red];
                break;
            case IngredientType.Orange:
                _sr.sprite = frame ?_frame01[(int)IngredientType.Orange] :_frame02[(int)IngredientType.Orange];
                break;
            case IngredientType.Blue:
                _sr.sprite = frame ? _frame01[(int)IngredientType.Blue] : _frame02[(int)IngredientType.Blue];
                break;
            case IngredientType.Pink:
                _sr.sprite = frame ?_frame01[(int)IngredientType.Pink] :_frame02[(int)IngredientType.Pink];
                break;
        }
    }

    private IEnumerator AnimateSprite()
    {
        while (_repeatCount > 0)
        {
            SetSprite(_isFrame01);
            _isFrame01 = !_isFrame01;
            yield return new WaitForSeconds(0.2f);
            _repeatCount--;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<PlayerStats>() != null)
        {
            GameManager.Instance.AddIngredientCount((int)_ingredientType, 1);
            Destroy(gameObject);
        }
    }
}
