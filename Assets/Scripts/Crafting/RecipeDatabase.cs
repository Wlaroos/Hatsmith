using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRecipeDatabase", menuName = "Crafting/Recipe Database")]
public class RecipeDatabase : ScriptableObject
{
    public List<RecipeData> recipeList;
}
