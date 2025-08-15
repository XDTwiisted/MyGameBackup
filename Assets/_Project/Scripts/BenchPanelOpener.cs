using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BenchPanelOpener : MonoBehaviour
{
    [Header("Bench Buttons")]
    public Button craftingButton;
    public Button repairButton;
    public Button cookingButton;
    public Button waterPurifierButton;

    [Header("Overlay")]
    public GameObject stationOverlayPanel;   // Your StationOverlayPanel (inactive by default)
    public Button backButton;                // Back button inside StationOverlayPanel

    [Header("Optional Overlay Content")]
    public TextMeshProUGUI stationTitle;     // Optional: title text in overlay
    public Image stationIcon;                // Optional: icon image in overlay
    public Sprite craftingIcon;              // Optional: per-station icons
    public Sprite repairIcon;
    public Sprite cookingIcon;
    public Sprite waterPurifierIcon;

    private void Start()
    {
        // Hide overlay at start
        if (stationOverlayPanel != null)
            stationOverlayPanel.SetActive(false);

        // Wire station openers
        if (craftingButton != null)
            craftingButton.onClick.AddListener(() => OpenOverlay("Crafting Station", craftingIcon));

        if (repairButton != null)
            repairButton.onClick.AddListener(() => OpenOverlay("Repair Bench", repairIcon));

        if (cookingButton != null)
            cookingButton.onClick.AddListener(() => OpenOverlay("Cooking Station", cookingIcon));

        if (waterPurifierButton != null)
            waterPurifierButton.onClick.AddListener(() => OpenOverlay("Water Purifier", waterPurifierIcon));

        // Wire back/close
        if (backButton != null)
            backButton.onClick.AddListener(CloseOverlay);
    }

    private void OpenOverlay(string title, Sprite icon)
    {
        if (stationOverlayPanel != null)
            stationOverlayPanel.SetActive(true);

        // Optional UI updates (safe if unassigned)
        if (stationTitle != null)
            stationTitle.text = title;

        if (stationIcon != null)
            stationIcon.sprite = icon;
    }

    public void CloseOverlay()
    {
        if (stationOverlayPanel != null)
            stationOverlayPanel.SetActive(false);
    }
}
