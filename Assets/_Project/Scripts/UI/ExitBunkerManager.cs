using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;

public class ExitBunkerManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button exitBunkerButton;
    public Button returnButton;
    public Button subtractOneHourButton;
    public Button confirmExplorationButton;
    public Button cancelExplorationButton;
    public TextMeshProUGUI explorationTimerText;
    public Slider staminaBar;

    [Header("Panels")]
    public GameObject gearUpPanel;
    public GameObject stashButton;
    public GameObject inventoryButton;
    public GameObject stashPanel;
    public GameObject inventoryPanel;

    [Header("Character")]
    public GameObject character;

    [Header("Exploration")]
    public ExplorationDialogueManager explorationDialogue;
    public ExplorationManager explorationManager;
    public LootTable lootTable;

    [Header("Stamina Boost")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 10f;
    public float speedMultiplier = 5f;

    private float currentStamina;
    private float timer = 0f;
    private bool isCountingUp = false;
    private bool isCountingDown = false;
    private bool isExploring = false;
    private bool isHoldingScreen = false;

    private const string STATE_KEY = "gameState";
    private const string EXPLORE_TIME_KEY = "explorationStartTime";
    private const string RETURN_TIME_KEY = "returnStartTime";
    private const string RETURN_DURATION_KEY = "returnDuration";

    public float CurrentSpeedMultiplier => isHoldingScreen && currentStamina > 0f ? speedMultiplier : 1f;

    void Start()
    {
        returnButton.gameObject.SetActive(false);
        explorationTimerText.text = "Time Outside: 00:00";

        currentStamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = maxStamina;
        staminaBar.gameObject.SetActive(false);

        RestoreState();

        returnButton.onClick.AddListener(StartReturnTimer);
        if (subtractOneHourButton != null)
            subtractOneHourButton.onClick.AddListener(SubtractOneHour);

        if (exitBunkerButton != null)
        {
            exitBunkerButton.onClick.RemoveAllListeners();
            exitBunkerButton.onClick.AddListener(ToggleGearUpPanel);
        }

        if (stashButton != null)
        {
            stashButton.GetComponent<Button>().onClick.RemoveAllListeners();
            stashButton.GetComponent<Button>().onClick.AddListener(() => ToggleStashPanel());
        }

        if (confirmExplorationButton != null)
        {
            confirmExplorationButton.onClick.RemoveAllListeners();
            confirmExplorationButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetString(EXPLORE_TIME_KEY, DateTime.UtcNow.ToBinary().ToString());
                PlayerPrefs.SetString(STATE_KEY, "exploring");
                PlayerPrefs.Save();
                SetExplorationUIState(true);
                StartExplorationInternal();
            });
        }

        if (cancelExplorationButton != null)
        {
            cancelExplorationButton.onClick.RemoveAllListeners();
            cancelExplorationButton.onClick.AddListener(() => gearUpPanel.SetActive(false));
        }

        if (gearUpPanel != null)
            gearUpPanel.SetActive(false);
    }

    private void ToggleGearUpPanel()
    {
        if (gearUpPanel != null)
            gearUpPanel.SetActive(!gearUpPanel.activeSelf);
    }

    public void ToggleStashPanel()
    {
        if (stashPanel == null || inventoryPanel == null)
            return;

        bool stashIsOpen = stashPanel.activeSelf;

        inventoryPanel.SetActive(false);
        stashPanel.SetActive(!stashIsOpen);

        if (!stashIsOpen)
            StashManagerUI.Instance?.RefreshStashUI();
    }

    private void SetExplorationUIState(bool exploring)
    {
        isExploring = exploring;

        if (stashButton != null) stashButton.SetActive(!exploring);
        if (stashPanel != null) stashPanel.SetActive(false);

        if (inventoryButton != null) inventoryButton.SetActive(exploring);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void StartExplorationInternal(float restoredTime = 0f)
    {
        gearUpPanel?.SetActive(false);
        character.SetActive(false);
        exitBunkerButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(true);

        timer = restoredTime;
        isCountingUp = true;
        isCountingDown = false;

        staminaBar.gameObject.SetActive(true);
        currentStamina = maxStamina;
        staminaBar.value = currentStamina;

        explorationDialogue?.StartExploration();
        explorationManager?.StartExploring();
    }

    private void RestoreState()
    {
        string state = PlayerPrefs.GetString(STATE_KEY, "bunker");

        if (state == "exploring")
        {
            if (PlayerPrefs.HasKey(EXPLORE_TIME_KEY))
            {
                long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(EXPLORE_TIME_KEY));
                DateTime savedTime = DateTime.FromBinary(binaryTime);
                TimeSpan elapsed = DateTime.UtcNow - savedTime;

                Debug.Log($"Restoring exploration. Elapsed time: {elapsed.TotalSeconds} seconds");

                SetExplorationUIState(true);
                StartExplorationInternal((float)elapsed.TotalSeconds);
            }
            else
            {
                Debug.LogWarning("Exploration time key not found, starting in bunker.");
                SetExplorationUIState(false);
            }
        }
        else
        {
            Debug.Log("Restoring state: Bunker");
            SetExplorationUIState(false);
        }
    }

    private void StartReturnTimer()
    {
        isCountingDown = true;
        isCountingUp = false;

        float returnDuration = 60f;
        timer = returnDuration;

        PlayerPrefs.SetString(STATE_KEY, "returning");
        PlayerPrefs.SetString(RETURN_TIME_KEY, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, returnDuration);
        PlayerPrefs.Save();

        Debug.Log("Started return timer.");
    }

    private void SubtractOneHour()
    {
        timer = Mathf.Max(0f, timer - 3600f);
        Debug.Log("Subtracted 1 hour from return timer.");
    }

    void Update()
    {
        HandleInput();

        if (isCountingUp)
        {
            float delta = Time.deltaTime * CurrentSpeedMultiplier;
            timer += delta;
            UpdateTimerDisplay(timer);

            explorationDialogue?.UpdateDialogue(delta);
            explorationManager?.AdvanceExplorationTime(delta);

            if (isHoldingScreen && currentStamina > 0f)
                currentStamina -= staminaDrainRate * Time.deltaTime;
            else
                currentStamina += staminaRegenRate * Time.deltaTime;

            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            staminaBar.value = currentStamina;
        }
        else if (isCountingDown)
        {
            float delta = Time.deltaTime * CurrentSpeedMultiplier;
            timer -= delta;

            if (isHoldingScreen && currentStamina > 0f)
                currentStamina -= staminaDrainRate * Time.deltaTime;
            else
                currentStamina += staminaRegenRate * Time.deltaTime;

            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            staminaBar.value = currentStamina;

            if (timer <= 0f)
            {
                timer = 0f;
                isCountingDown = false;

                character.SetActive(true);
                exitBunkerButton.gameObject.SetActive(true);
                returnButton.gameObject.SetActive(false);
                staminaBar.gameObject.SetActive(false);

                PlayerPrefs.SetString(STATE_KEY, "bunker");
                PlayerPrefs.Save();

                explorationDialogue?.ClearDialogue();
                SetExplorationUIState(false);

                if (inventoryPanel != null && inventoryPanel.activeSelf)
                    inventoryPanel.SetActive(false);

                ReturnExplorationLoot();

                Debug.Log("Return timer completed: Player is back in bunker.");
            }

            UpdateTimerDisplay(timer);
        }
    }

    private void ReturnExplorationLoot()
    {
        var allItems = InventoryManager.Instance?.GetAllItems();

        if (allItems == null || (allItems.Value.Item1.Count == 0 && allItems.Value.Item2.Count == 0))
        {
            Debug.Log("No loot to transfer to stash.");
            return;
        }

        StashManager.Instance?.AddItems(allItems.Value.Item1, allItems.Value.Item2);
        InventoryManager.Instance?.ClearAllItems();

        Debug.Log($"Transferred {allItems.Value.Item1.Count} stackable and {allItems.Value.Item2.Count} durable items to stash.");
    }


    void HandleInput()
    {
        bool holding = false;

        if (Screen.height > 0)
        {
            float yPos = 0f;

            if (Mouse.current != null && Mouse.current.leftButton.isPressed)
            {
                yPos = Mouse.current.position.ReadValue().y;
                if (yPos > Screen.height * 0.15f && yPos < Screen.height * 0.85f)
                    holding = true;
            }

            if (!holding && Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                yPos = Touchscreen.current.primaryTouch.position.ReadValue().y;
                if (yPos > Screen.height * 0.15f && yPos < Screen.height * 0.85f)
                    holding = true;
            }
        }

        isHoldingScreen = holding;
    }

    void UpdateTimerDisplay(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        explorationTimerText.text = $"Time Outside: {hours:00}:{minutes:00}:{seconds:00}";
    }
}
