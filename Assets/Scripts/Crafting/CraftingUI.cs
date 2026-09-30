using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private RecipeDatabase _database;
    [SerializeField] private IngredientDatabase _ingredientDatabase;
    [SerializeField] private GameObject _recipeUIPrefab;
    [SerializeField] private Transform _recipeContainer;
    [SerializeField] private GameObject _invHolder;

    private List<RecipeData> _recipes = new List<RecipeData>();

    private void Awake()
    {
        if (_database == null || _recipeUIPrefab == null || _ingredientDatabase == null)
        {
            Debug.LogError("CraftingUI: Missing assigned references in Inspector!", this);
            return;
        }

        // Initialize Recipes
        foreach (RecipeData recipe in _database.recipeList)
        {
            if (recipe == null) continue;

            GameObject prefab = Instantiate(_recipeUIPrefab, _recipeContainer);

            if (prefab.TryGetComponent(out HatRecipeUI recipeUI))
            {
                recipeUI.InitRecipe(recipe);
            }

            _recipes.Add(recipe);
        }
    }

    private void OnEnable()
    {
        GameManager.Instance.IngredientPickedUpEvent += UpdateInventoryUI;
    }
    private void OnDisable()
    {
        GameManager.Instance.IngredientPickedUpEvent -= UpdateInventoryUI;
    }

    private void Start()
    {
        UpdateInventoryUI();
    }

    public void UpdateInventoryUI()
    {
        Transform holderTransform = _invHolder.transform;

        for (int i = 0; i < _ingredientDatabase.count; i++)
        {
            int spriteChildIndex = i * 2;
            int countChildIndex = i * 2 + 1;

            // Ensure inv has enough child elements
            if (countChildIndex >= holderTransform.childCount)
            {
                Debug.LogWarning("CraftingUI: Not enough UI child elements in _invHolder for all ingredients!");
                break;
            }

            IngredientData ingredient = _ingredientDatabase.ingredientDataList[i];

            // Set the Sprite on the first child
            Transform spriteChild = holderTransform.GetChild(spriteChildIndex);
            if (spriteChild.TryGetComponent<Image>(out Image img))
            {
                img.sprite = ingredient.frames[0]; 
            }

            // Set the Count on the second child
            Transform countChild = holderTransform.GetChild(countChildIndex);
            if (countChild.TryGetComponent<TMP_Text>(out TMP_Text textMesh))
            {
                int currentCount = CraftingManager.Instance.GetIngredientCount(ingredient);
                textMesh.text = currentCount.ToString();
                textMesh.color = ingredient.color;
            }
        }
    }
}