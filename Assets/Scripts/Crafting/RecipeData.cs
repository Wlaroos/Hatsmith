using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipeData", menuName = "Crafting/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [System.Serializable]
    public struct IngredientRequirement
    {
        public IngredientData ingredientData;
        public int amount;
    }

    [Header("Recipe Requirements")]
    public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();

    [Header("Resulting Hat")]
    public HatData resultHat;
}