using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredientDatabase", menuName = "Crafting/Ingredient Database")]
public class IngredientDatabase : ScriptableObject
{
    public List<IngredientData> ingredientDataList = new List<IngredientData>();
    public int count => ingredientDataList.Count;
}
