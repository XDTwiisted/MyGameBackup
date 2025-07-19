using UnityEngine;

public class InventoryUseHandler : MonoBehaviour
{
    public PlayerStats playerStats;  // Assign in Inspector

    public void UseItem(InventoryItemData item)
    {
        if (item == null || playerStats == null)
        {
            Debug.LogWarning("UseItem failed: item or playerStats is null.");
            return;
        }

        // Apply hunger restoration if any
        if (item.restoreHunger > 0)
        {
            playerStats.RestoreHunger(item.restoreHunger);
        }

        // Apply thirst restoration if any
        if (item.restoreThirst > 0)
        {
            playerStats.RestoreThirst(item.restoreThirst);
        }

        // Decrease quantity
        item.quantity--;

        // Remove from list if quantity is 0 or less
        if (item.quantity <= 0 && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.allItems.Remove(item);
        }

        // Refresh UI
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RefreshInventoryUI();
        }
    }
}
