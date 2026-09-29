using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipeData", menuName = "Crafting/Recipe Data")]
public class RecipeData : ScriptableObject
{
    [System.Serializable]
    public struct IngredientRequirement
    {
        public IngredientData ingredient;
        public int count;
    }

    [Header("Recipe Requirements")]
    public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();

    [Header("Resulting Hat")]
    public HatData resultHat;
}