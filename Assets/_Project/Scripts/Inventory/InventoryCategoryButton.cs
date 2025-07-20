using UnityEngine;
using UnityEngine.UI;

public class InventoryCategoryButton : MonoBehaviour
{
    public string categoryName;  // Set in Inspector, e.g. "Food", "Tools", "Weapons", "Misc"
    public Button button;        // Assign the button component in Inspector (optional, will auto-get if null)
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.white;

    private Image buttonImage;

    private void Awake()
    {
        // Get the Button component if not assigned
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
            Debug.LogError("InventoryCategoryButton: No Button component found!");

        // Get the Image component from the button (background)
        if (button != null)
            buttonImage = button.GetComponent<Image>();

        if (buttonImage == null)
            Debug.LogWarning("InventoryCategoryButton: Button has no Image component!");

        // Add listener for click event
        if (button != null)
            button.onClick.AddListener(OnClick);

        // Set initial color as inactive
        SetInactive();
    }

    private void OnClick()
    {
        // Tell InventoryManager to set the category filter
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.SetCategory(categoryName);
        else
            Debug.LogWarning("InventoryCategoryButton: InventoryManager instance not found.");

        // Tell InventoryCategoryGroup to update button highlights
        if (InventoryCategoryGroup.Instance != null)
            InventoryCategoryGroup.Instance.SetActiveCategory(categoryName);
        else
            Debug.LogWarning("InventoryCategoryButton: InventoryCategoryGroup instance not found.");
    }

    public void SetActive()
    {
        if (buttonImage != null)
            buttonImage.color = activeColor;
    }

    public void SetInactive()
    {
        if (buttonImage != null)
            buttonImage.color = inactiveColor;
    }
}
