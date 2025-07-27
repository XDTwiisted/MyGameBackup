using UnityEngine;
using UnityEngine.UI;

public class DebugControls : MonoBehaviour
{
    public Button clearInventoryButton;

    void Start()
    {
        if (clearInventoryButton != null)
        {
            clearInventoryButton.onClick.AddListener(ClearInventory);
        }
    }

    void ClearInventory()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearInventory();
        }
        else
        {
            Debug.LogWarning("InventoryManager instance not found.");
        }
    }
}
