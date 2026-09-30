using UnityEngine;
using System.Collections.Generic;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] private RecipeDatabase _database;
    [SerializeField] private GameObject _recipeUIPrefab;
    [SerializeField] private Transform _recipeContainer;
    private List<RecipeData> _recipes = new List<RecipeData>();

    private void Awake()
    {
        if (_database == null || _recipeUIPrefab == null)
        {
            Debug.LogError("CraftingUI: Missing assigned references in Inspector!", this);
            return;
        }

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
}