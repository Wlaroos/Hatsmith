using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HatRecipeUI : MonoBehaviour
{
    [SerializeField] Image _hatImage;
    [SerializeField] Image[] _hatIngredients;
    [SerializeField] TextMeshProUGUI _hatName;
    [SerializeField] TextMeshProUGUI _hatDescription;
    [SerializeField] Button _craftButton;

    [Header("Crafting Button Colors")]
    [SerializeField] private Color _craftableColor = Color.green;
    [SerializeField] private Color _uncraftableColor = Color.grey;

    private RecipeData _currentRecipe;

    private void Awake()
    {
        if (_craftButton != null)
        {
            _craftButton.onClick.AddListener(OnCraftButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (_craftButton != null)
        {
            _craftButton.onClick.RemoveListener(OnCraftButtonClicked);
        }
    }

    public void InitRecipe(RecipeData recipeData)
    {
        _currentRecipe = recipeData;

        HatData hatData = recipeData.resultHat;

        if (hatData != null)
        {
            if (_hatImage != null) _hatImage.sprite = hatData.hatSprite;
            if (_hatName != null) _hatName.text = hatData.hatName;
            if (_hatDescription != null) _hatDescription.text = hatData.hatDescription;
        }

        // Populate ingredient UI slots based on amount
        int ingredientUIIndex = 0;

        if (recipeData.requiredIngredients != null)
        {
            foreach (RecipeData.IngredientRequirement ingredientReq in recipeData.requiredIngredients)
            {
                if (ingredientReq.ingredientData == null || ingredientReq.ingredientData.frames.Length == 0)
                    continue;

                // Frame one of the ingredients
                Sprite ingredientSprite = ingredientReq.ingredientData.frames[0];

                // Enable and set sprite for as many slots as required by ingredient amount
                for (int i = 0; i < ingredientReq.amount; i++)
                {
                    if (ingredientUIIndex < _hatIngredients.Length)
                    {
                        _hatIngredients[ingredientUIIndex].gameObject.SetActive(true);
                        _hatIngredients[ingredientUIIndex].sprite = ingredientSprite;
                        ingredientUIIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        // Disable unused ingredient slots
        for (int i = ingredientUIIndex; i < _hatIngredients.Length; i++)
        {
            if (_hatIngredients[i] != null)
            {
                _hatIngredients[i].gameObject.SetActive(false);
            }
        }

        UpdateCraftButtonState();
    }

    private void Update()
    {
        UpdateCraftButtonState();
    }

    private void UpdateCraftButtonState()
    {
        if (_craftButton == null || _currentRecipe == null || CraftingManager.Instance == null)
            return;

        bool canCraft = CraftingManager.Instance.CanCraft(_currentRecipe);

        // Control interactable
        _craftButton.interactable = canCraft;

        // Change button color
        Graphic buttonGraphic = _craftButton.targetGraphic != null ? _craftButton.targetGraphic : _craftButton.GetComponent<Image>();
        if (buttonGraphic != null)
        {
            buttonGraphic.color = canCraft ? _craftableColor : _uncraftableColor;
        }
    }

    private void OnCraftButtonClicked()
    {
        if (_currentRecipe != null && CraftingManager.Instance != null && CraftingManager.Instance.CanCraft(_currentRecipe))
        {
            CraftingManager.Instance.CraftHat(_currentRecipe);
        }
    }
}