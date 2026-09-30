using UnityEngine;

public class CraftingTable : MonoBehaviour
{
    [SerializeField] private CanvasGroup _craftingMenuCanvasGroup;
    private bool _showE;
    private void Update()
    {
        if(_showE && Input.GetKeyDown(KeyCode.E))
        {
            _craftingMenuCanvasGroup.alpha = 1;
            _craftingMenuCanvasGroup.interactable = true;
            _craftingMenuCanvasGroup.blocksRaycasts = true;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.TryGetComponent(out PlayerHealth player);
            _showE = true;
            player.ShowE(_showE);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            collision.TryGetComponent(out PlayerHealth player);
            _showE = false;
            player.ShowE(_showE);
        }
    }

}
