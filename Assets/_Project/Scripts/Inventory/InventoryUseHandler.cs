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

        // Apply item effects
        if (item.restoreHunger > 0)
            playerStats.RestoreHunger(item.restoreHunger);

        if (item.restoreThirst > 0)
            playerStats.RestoreThirst(item.restoreThirst);

        // Reduce quantity via InventoryManager
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.UseItem(item);
        }
    }
}
