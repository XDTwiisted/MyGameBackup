using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject inventoryPanel; // Assign your inventory UI panel in the Inspector

    private bool isVisible = false;

    public void ToggleInventory()
    {
        isVisible = !isVisible;
        inventoryPanel.SetActive(isVisible);
    }
}
