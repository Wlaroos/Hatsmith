using System.Collections.Generic;
using UnityEngine;

public class CraftingManager : MonoBehaviour
{
    public static CraftingManager Instance { get; private set; }

    [Header("Ingredient Database")]
    [SerializeField] private IngredientDatabase _ingredientDatabase;

    [Header("Hat Pickup")]
    [SerializeField] private GameObject _hatPickupPrefab;

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
        if (_ingredientPrefab == null || _ingredientDatabase.count == 0) return;
        if (Random.value > _ingredientDropChance) return;

        // Pick a random ingredient SO from the list
        IngredientData randomData = _ingredientDatabase.ingredientDataList[Random.Range(0, _ingredientDatabase.count)];

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
            if (req.ingredientData == null) continue;
            if (GetIngredientCount(req.ingredientData) < req.amount)
            {
                return false;
            }
        }

        return true;
    }

    public void CraftHat(RecipeData recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.LogWarning($"[CraftingManager] Insufficient ingredients to craft {recipe.name}");
            return;
        }

        // Subtract ingredients
        foreach (var req in recipe.requiredIngredients)
        {
            if (req.ingredientData != null)
            {
                _inventory[req.ingredientData] -= req.amount;
            }
        }

        Debug.Log($"Successfully crafted: {recipe.resultHat.hatName}");

        GameManager.Instance.InvokeHatCraftedEvent(recipe.resultHat);

        GameObject hatPickupPrefab = Instantiate(_hatPickupPrefab, FindFirstObjectByType<CraftingTable>().transform);

        hatPickupPrefab.TryGetComponent(out HatPickup hatPickup);
        hatPickup.SetHatData(recipe.resultHat);
    }
}