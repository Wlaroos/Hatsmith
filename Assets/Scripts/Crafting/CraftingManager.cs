using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("Available Ingredients")]
    [SerializeField] private List<IngredientData> _availableIngredients = new List<IngredientData>();

    [Header("Drop Settings")]
    [SerializeField] private GameObject _ingredientPrefab;
    [SerializeField] private float _ingredientDropChance = 0.25f;
    [SerializeField] private float _animationFrameRate = 0.2f;

    // Dictionary tracks count dynamically per ScriptableObject asset reference
    private Dictionary<IngredientData, int> _inventory = new Dictionary<IngredientData, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddIngredient(IngredientData ingredient, int amount)
    {
        if (ingredient == null) return;

        if (_inventory.ContainsKey(ingredient))
        {
            _inventory[ingredient] += amount;
        }
        else
        {
            _inventory.Add(ingredient, amount);
        }

        Debug.Log($"Added {amount}x {ingredient.ingredientName}. Total: {_inventory[ingredient]}");
    }

    public int GetIngredientCount(IngredientData ingredient)
    {
        return _inventory.TryGetValue(ingredient, out int count) ? count : 0;
    }

    public void SpawnIngredient(Transform parent)
    {
        if (_ingredientPrefab == null || _availableIngredients.Count == 0) return;
        if (Random.value > _ingredientDropChance) return;

        // Pick a random ingredient SO from the list
        IngredientData randomData = _availableIngredients[Random.Range(0, _availableIngredients.Count)];

        GameObject obj = Instantiate(_ingredientPrefab, parent.position, Quaternion.identity);

        if (obj.TryGetComponent<IngredientPickup>(out var pickup))
        {
            pickup.Initialize(randomData, _animationFrameRate);
            GameManager.Instance.InvokeIngredientSpawnedEvent(randomData);
        }
    }

    public bool CanCraft(RecipeData recipe)
    {
        if (recipe == null) return false;

        foreach (var req in recipe.requiredIngredients)
        {
            if (req.ingredient == null) continue;
            if (GetIngredientCount(req.ingredient) < req.count)
            {
                return false;
            }
        }

        return true;
    }

    public HatData CraftHat(RecipeData recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.LogWarning($"[CraftingManager] Insufficient ingredients to craft {recipe.name}");
            return null;
        }

        // Subtract ingredients
        foreach (var req in recipe.requiredIngredients)
        {
            if (req.ingredient != null)
            {
                _inventory[req.ingredient] -= req.count;
            }
        }

        Debug.Log($"Successfully crafted: {recipe.resultHat.hatName}");
        return recipe.resultHat;
    }
}