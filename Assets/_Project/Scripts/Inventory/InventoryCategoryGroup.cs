using UnityEngine;
using System.Collections.Generic;

public class InventoryCategoryGroup : MonoBehaviour
{
    public static InventoryCategoryGroup Instance { get; private set; }

    public List<InventoryCategoryButton> categoryButtons;

    private string currentCategory = "";

    public string CurrentCategory => currentCategory;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (categoryButtons == null || categoryButtons.Count == 0)
        {
            Debug.LogWarning("InventoryCategoryGroup: No category buttons assigned.");
            return;
        }

        string startCategory;

        if (InventoryManager.Instance != null && !string.IsNullOrEmpty(InventoryManager.Instance.CurrentCategory))
        {
            startCategory = InventoryManager.Instance.CurrentCategory;
        }
        else
        {
            // Fallback to first button's category
            startCategory = !string.IsNullOrEmpty(categoryButtons[0].categoryName) ?
                            categoryButtons[0].categoryName : "Food";
        }

        SetActiveCategory(startCategory);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetCategory(startCategory);
        }
        else
        {
            Debug.LogWarning("InventoryCategoryGroup: InventoryManager instance not found.");
        }
    }

    /// <summary>
    /// Call this method to refresh button highlights (e.g. when opening inventory)
    /// </summary>
    public void RefreshCategoryButtons()
    {
        SetActiveCategory(currentCategory);
    }

    public void SetActiveCategory(string categoryName)
    {
        currentCategory = categoryName;

        foreach (var btn in categoryButtons)
        {
            if (btn == null) continue;

            if (btn.categoryName == categoryName)
                btn.SetActive();
            else
                btn.SetInactive();
        }
    }
}
