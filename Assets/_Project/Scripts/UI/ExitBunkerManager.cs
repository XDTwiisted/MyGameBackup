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
    private bool staminaLocked = false;
    private bool staminaMustFullyRecharge = false;

    private const string STATE_KEY = "gameState";
    private const string EXPLORE_TIME_KEY = "explorationStartTime";
    private const string RETURN_TIME_KEY = "returnStartTime";
    private const string RETURN_DURATION_KEY = "returnDuration";

    public float CurrentSpeedMultiplier => isHoldingScreen && !staminaLocked ? speedMultiplier : 1f;

    void Start()
    {
        returnButton.gameObject.SetActive(false);
        explorationTimerText.text = "Time Outside: 00:00:00";

        currentStamina = maxStamina;
        staminaBar.maxValue = maxStamina;
        staminaBar.value = maxStamina;
        staminaBar.gameObject.SetActive(false);

        if (explorationDialogue != null && explorationDialogue.dialogueText != null)
            explorationDialogue.dialogueText.enabled = false;

        RestoreState();

        returnButton.onClick.AddListener(StartReturnTimer);
        subtractOneHourButton?.onClick.AddListener(SubtractOneHour);

        exitBunkerButton?.onClick.RemoveAllListeners();
        exitBunkerButton?.onClick.AddListener(ToggleGearUpPanel);

        stashButton?.GetComponent<Button>().onClick.RemoveAllListeners();
        stashButton?.GetComponent<Button>().onClick.AddListener(() => ToggleStashPanel());

        confirmExplorationButton?.onClick.RemoveAllListeners();
        confirmExplorationButton?.onClick.AddListener(() =>
        {
            PlayerPrefs.SetString(EXPLORE_TIME_KEY, DateTime.UtcNow.ToBinary().ToString());
            PlayerPrefs.SetString(STATE_KEY, "exploring");
            PlayerPrefs.Save();
            SetExplorationUIState(true);
            StartExplorationInternal();
        });

        cancelExplorationButton?.onClick.RemoveAllListeners();
        cancelExplorationButton?.onClick.AddListener(() => gearUpPanel.SetActive(false));

        gearUpPanel?.SetActive(false);
    }

    void Update()
    {
        HandleInput();

        if (isCountingUp)
        {
            float delta = Time.deltaTime * CurrentSpeedMultiplier;
            timer += delta;
            UpdateTimerDisplay(timer);

            DateTime simulatedStartTime = DateTime.UtcNow - TimeSpan.FromSeconds(timer);
            PlayerPrefs.SetString(EXPLORE_TIME_KEY, simulatedStartTime.ToBinary().ToString());
            PlayerPrefs.Save();

            explorationDialogue?.UpdateDialogue(delta);
            explorationManager?.AdvanceExplorationTime(delta);
        }

        if (isCountingDown)
        {
            float delta = Time.deltaTime * CurrentSpeedMultiplier;
            timer -= delta;

            if (timer <= 0f)
            {
                timer = 0f;
                isCountingDown = false;
                FinishReturn();
            }

            UpdateTimerDisplay(timer);

            DateTime simulatedReturnStart = DateTime.UtcNow - TimeSpan.FromSeconds(timer);
            PlayerPrefs.SetString(RETURN_TIME_KEY, simulatedReturnStart.ToBinary().ToString());
            PlayerPrefs.SetFloat(RETURN_DURATION_KEY, timer);
            PlayerPrefs.Save();
        }

        if (isExploring && staminaBar.gameObject.activeSelf)
        {
            if (isHoldingScreen && currentStamina > 0f && !staminaMustFullyRecharge)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                if (currentStamina <= 0f)
                {
                    currentStamina = 0f;
                    staminaLocked = true;
                    staminaMustFullyRecharge = true;
                }
            }
            else
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina >= maxStamina)
                {
                    currentStamina = maxStamina;
                    staminaLocked = false;
                    staminaMustFullyRecharge = false;
                }
            }

            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            staminaBar.value = currentStamina;
        }
    }

    private void HandleInput()
    {
        isHoldingScreen = false;

        if (inventoryPanel != null && inventoryPanel.activeSelf)
            return;

        float screenHeight = Screen.height;
        float minY = screenHeight * 0.20f;
        float maxY = screenHeight * 0.80f;
        float yPos = 0f;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            yPos = Touchscreen.current.primaryTouch.position.ReadValue().y;
            if (yPos > minY && yPos < maxY)
                isHoldingScreen = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            yPos = Mouse.current.position.ReadValue().y;
            if (yPos > minY && yPos < maxY)
                isHoldingScreen = true;
        }
    }

    private void ToggleGearUpPanel()
    {
        gearUpPanel?.SetActive(!gearUpPanel.activeSelf);
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

        stashButton?.SetActive(!exploring);
        stashPanel?.SetActive(false);
        inventoryButton?.SetActive(exploring);
        inventoryPanel?.SetActive(false);

        if (explorationDialogue != null && explorationDialogue.dialogueText != null)
            explorationDialogue.dialogueText.enabled = exploring || isCountingDown;
    }

    private void StartExplorationInternal(float restoredTime = 0f)
    {
        gearUpPanel?.SetActive(false);
        // We no longer hide the character on exploration start
        exitBunkerButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(true);

        timer = restoredTime;
        isCountingUp = true;
        isCountingDown = false;

        staminaBar.gameObject.SetActive(true);
        currentStamina = maxStamina;
        staminaBar.value = currentStamina;
        staminaLocked = false;

        if (explorationDialogue != null)
        {
            if (explorationDialogue.dialogueText != null)
                explorationDialogue.dialogueText.enabled = true;

            explorationDialogue.Refresh();
            explorationDialogue.StartExploration();
        }

        explorationManager?.StartExploring();
    }

    private void RestoreState()
    {
        string state = PlayerPrefs.GetString(STATE_KEY, "bunker");

        if (state == "exploring" && PlayerPrefs.HasKey(EXPLORE_TIME_KEY))
        {
            long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(EXPLORE_TIME_KEY));
            DateTime savedTime = DateTime.FromBinary(binaryTime);
            TimeSpan elapsed = DateTime.UtcNow - savedTime;

            SetExplorationUIState(true);
            StartExplorationInternal((float)elapsed.TotalSeconds);
        }
        else
        {
            SetExplorationUIState(false);
        }
    }

    private void StartReturnTimer()
    {
        isCountingDown = true;
        isCountingUp = false;

        float returnDuration = timer;
        timer = returnDuration;

        PlayerPrefs.SetString(STATE_KEY, "returning");
        PlayerPrefs.SetString(RETURN_TIME_KEY, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, returnDuration);
        PlayerPrefs.Save();

        returnButton.gameObject.SetActive(false);

        if (explorationDialogue != null && explorationDialogue.dialogueText != null)
            explorationDialogue.dialogueText.enabled = false;
    }

    private void SubtractOneHour()
    {
        timer = Mathf.Max(0f, timer - 3600f);
    }

    private void UpdateTimerDisplay(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        explorationTimerText.text = $"Time Outside: {hours:00}:{minutes:00}:{seconds:00}";
    }

    private void FinishReturn()
    {
        isCountingDown = false;
        isCountingUp = false;
        isExploring = false;

        PlayerPrefs.SetString(STATE_KEY, "bunker");
        PlayerPrefs.DeleteKey(EXPLORE_TIME_KEY);
        PlayerPrefs.DeleteKey(RETURN_TIME_KEY);
        PlayerPrefs.DeleteKey(RETURN_DURATION_KEY);
        PlayerPrefs.Save();

        explorationManager?.StopExploring();

        returnButton.gameObject.SetActive(false);
        exitBunkerButton.gameObject.SetActive(true);
        character.SetActive(true);
        staminaBar.gameObject.SetActive(false);

        if (explorationDialogue != null && explorationDialogue.dialogueText != null)
            explorationDialogue.dialogueText.enabled = false;

        var items = InventoryManager.Instance?.GetAllItems();
        if (items != null)
        {
            StashManager.Instance?.AddItems(items.Value.Item1, items.Value.Item2);
        }
        InventoryManager.Instance?.ClearInventory();

        SetExplorationUIState(false);
    }
}
