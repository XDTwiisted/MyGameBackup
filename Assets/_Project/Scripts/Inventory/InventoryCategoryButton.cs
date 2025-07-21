using UnityEngine;
using UnityEngine.UI;

public class InventoryCategoryButton : MonoBehaviour
{
    public string categoryName;        // Set in Inspector, e.g. "Food", "Weapon", etc.
    public Button button;              // Optional: will auto-assign if null
    public Color activeColor = Color.green;
    public Color inactiveColor = Color.white;

    private Image buttonImage;

    private void Awake()
    {
        // Auto-assign the Button component if not set
        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("InventoryCategoryButton: No Button component found!");
            return;
        }

        // Get the Image component from the button (background)
        buttonImage = button.GetComponent<Image>();

        if (buttonImage == null)
            Debug.LogWarning("InventoryCategoryButton: Button has no Image component!");

        // Set up the button click listener
        button.onClick.AddListener(OnClick);

        // Set initial state as inactive
        SetInactive();
    }

    private void OnClick()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetCategory(categoryName);
        }
        else
        {
            Debug.LogWarning("InventoryCategoryButton: InventoryManager instance not found.");
        }

        if (InventoryCategoryGroup.Instance != null)
        {
            InventoryCategoryGroup.Instance.SetActiveCategory(categoryName);
        }
        else
        {
            Debug.LogWarning("InventoryCategoryButton: InventoryCategoryGroup instance not found.");
        }
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
