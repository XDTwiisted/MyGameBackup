using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryCategoryButton : MonoBehaviour
{
    [Header("Category")]
    public string categoryName;               // "Food", "Weapon", etc.
    public Button button;                     // Auto-assign if null

    [Header("Colors")]
    public Color activeBgColor = Color.white;
    public Color inactiveBgColor = new Color(1f, 1f, 1f, 0.15f);

    [Header("Optional Text Colors (if you have a TMP child)")]
    public Color activeTextColor = Color.black;
    public Color inactiveTextColor = new Color(1f, 1f, 1f, 0.7f);

    private Image bgImage;                    // button background image
    private TextMeshProUGUI labelTMP;         // optional child label

    private void Awake()
    {
        if (button == null) button = GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("InventoryCategoryButton: Button missing.");
            return;
        }

        bgImage = button.GetComponent<Image>();
        labelTMP = GetComponentInChildren<TextMeshProUGUI>(true);

        button.onClick.AddListener(OnClick);

        // Default to inactive; group will activate the chosen one
        SetInactive();
    }

    private void OnEnable()
    {
        // When panel re-opens, group will call SetActiveCategory again; until then keep visuals sane
        if (bgImage != null) bgImage.color = inactiveBgColor;
        if (labelTMP != null) labelTMP.color = inactiveTextColor;
    }

    private void OnClick()
    {
        var group = GetComponentInParent<InventoryCategoryGroup>();
        if (group != null)
        {
            group.SetActiveCategory(categoryName);
        }
        else
        {
            Debug.LogWarning("InventoryCategoryButton: No InventoryCategoryGroup found in parents.");
        }
    }

    // Called by group
    public void SetActive()
    {
        if (bgImage != null) bgImage.color = activeBgColor;
        if (labelTMP != null) labelTMP.color = activeTextColor;
    }

    // Called by group
    public void SetInactive()
    {
        if (bgImage != null) bgImage.color = inactiveBgColor;
        if (labelTMP != null) labelTMP.color = inactiveTextColor;
    }
}
