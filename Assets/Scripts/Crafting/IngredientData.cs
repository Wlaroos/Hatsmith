using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Crafting/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    [Header("General Info")]
    public string ingredientName;
    public Color color = Color.white;

    [Header("Animation Frames")]
    public Sprite[] frames;

    [Header("Effects")]
    public GameObject pickupEffectPrefab;
}