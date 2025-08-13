using UnityEngine;
using System.Collections.Generic;

public class InventoryCategoryGroup : MonoBehaviour
{
    [Tooltip("Buttons in this tab bar (order matters). If empty, will auto-find in children.")]
    public List<InventoryCategoryButton> categoryButtons = new List<InventoryCategoryButton>();

    [Tooltip("Optional explicit start category. If empty, uses first button's category.")]
    public string startCategory = "";

    private string currentCategory = "";
    public string CurrentCategory => currentCategory;

    private void Awake()
    {
        // Auto-find if not assigned
        if (categoryButtons == null || categoryButtons.Count == 0)
            categoryButtons = new List<InventoryCategoryButton>(GetComponentsInChildren<InventoryCategoryButton>(true));
    }

    private void OnEnable()
    {
        // Re-apply current visuals when panel becomes visible again
        if (!string.IsNullOrEmpty(currentCategory))
            SetActiveCategory(currentCategory);
    }

    private void Start()
    {
        if (categoryButtons == null || categoryButtons.Count == 0)
        {
            Debug.LogWarning("InventoryCategoryGroup: No category buttons assigned or found.");
            return;
        }

        var initial = !string.IsNullOrEmpty(startCategory)
                        ? startCategory
                        : (!string.IsNullOrEmpty(categoryButtons[0].categoryName)
                            ? categoryButtons[0].categoryName
                            : "Food");

        SetActiveCategory(initial);

        // Tell InventoryManager (if present) the chosen category
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetCategory(initial);
    }

    public void SetActiveCategory(string categoryName)
    {
        currentCategory = categoryName;

        // Update button visuals
        for (int i = 0; i < categoryButtons.Count; i++)
        {
            var btn = categoryButtons[i];
            if (btn == null) continue;
            if (btn.categoryName == categoryName) btn.SetActive();
            else btn.SetInactive();
        }

        // Notify InventoryManager (if this group is used for Inventory)
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetCategory(categoryName);
    }

    // Convenience if code elsewhere wants to select by index
    public void SetActiveIndex(int index)
    {
        if (index < 0 || index >= categoryButtons.Count) return;
        var name = categoryButtons[index]?.categoryName;
        if (!string.IsNullOrEmpty(name)) SetActiveCategory(name);
    }
}
