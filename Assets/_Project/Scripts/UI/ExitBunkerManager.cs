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
    public Button confirmExplorationButton;  // GearExploreButton
    public Button cancelExplorationButton;   // GearCancelButton
    public TextMeshProUGUI explorationTimerText;
    public Slider staminaBar;

    [Header("Panels")]
    public GameObject gearUpPanel;

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

        if (confirmExplorationButton != null)
        {
            confirmExplorationButton.onClick.RemoveAllListeners();
            confirmExplorationButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetString(EXPLORE_TIME_KEY, DateTime.UtcNow.ToBinary().ToString());
                PlayerPrefs.SetString(STATE_KEY, "exploring");
                PlayerPrefs.Save();
                StartExplorationInternal();
            });
        }

        if (cancelExplorationButton != null)
        {
            cancelExplorationButton.onClick.RemoveAllListeners();
            cancelExplorationButton.onClick.AddListener(() =>
            {
                gearUpPanel.SetActive(false);
            });
        }

        if (gearUpPanel != null)
            gearUpPanel.SetActive(false);
    }

    private void ToggleGearUpPanel()
    {
        if (gearUpPanel != null)
            gearUpPanel.SetActive(!gearUpPanel.activeSelf);
    }

    private void StartExplorationInternal(float restoredTime = 0f)
    {
        Debug.Log("StartExplorationInternal() called");

        gearUpPanel?.SetActive(false);
        character.SetActive(false);
        exitBunkerButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(true);

        timer = restoredTime;
        isCountingUp = true;
        isCountingDown = false;
        isExploring = true;

        staminaBar.gameObject.SetActive(true);
        currentStamina = maxStamina;
        staminaBar.value = currentStamina;

        explorationDialogue?.StartExploration();
        explorationManager?.StartExploring();
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
                Debug.Log("Return timer completed: Player is back in bunker.");
            }

            UpdateTimerDisplay(timer);
        }
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

    void RestoreState()
    {
        string state = PlayerPrefs.GetString(STATE_KEY, "bunker");
        Debug.Log($"Restoring state: {state}");

        if (state == "exploring")
        {
            if (PlayerPrefs.HasKey(EXPLORE_TIME_KEY))
            {
                long binary = Convert.ToInt64(PlayerPrefs.GetString(EXPLORE_TIME_KEY));
                DateTime exitTime = DateTime.FromBinary(binary);
                float restoredTime = (float)(DateTime.UtcNow - exitTime).TotalSeconds;

                StartExplorationInternal(restoredTime);
            }
        }
        else if (state == "returning")
        {
            if (PlayerPrefs.HasKey(RETURN_TIME_KEY) && PlayerPrefs.HasKey(RETURN_DURATION_KEY))
            {
                long binary = Convert.ToInt64(PlayerPrefs.GetString(RETURN_TIME_KEY));
                DateTime returnStart = DateTime.FromBinary(binary);
                float returnDuration = PlayerPrefs.GetFloat(RETURN_DURATION_KEY);

                TimeSpan timeSinceReturn = DateTime.UtcNow - returnStart;
                float timeLeft = returnDuration - (float)timeSinceReturn.TotalSeconds;

                if (timeLeft <= 0f)
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
                }
                else
                {
                    timer = timeLeft;
                    isCountingDown = true;
                    isCountingUp = false;
                    isExploring = false;

                    character.SetActive(false);
                    exitBunkerButton.gameObject.SetActive(false);
                    returnButton.gameObject.SetActive(true);

                    staminaBar.gameObject.SetActive(true);
                    currentStamina = maxStamina;
                    staminaBar.value = currentStamina;

                    explorationDialogue?.StopExploration();
                    explorationManager?.StopExploring();
                }
            }
        }
        else
        {
            timer = 0f;
            isCountingUp = false;
            isCountingDown = false;
            isExploring = false;

            character.SetActive(true);
            exitBunkerButton.gameObject.SetActive(true);
            returnButton.gameObject.SetActive(false);
            staminaBar.gameObject.SetActive(false);

            explorationDialogue?.ClearDialogue();
        }

        UpdateTimerDisplay(timer);
    }

    void StartReturnTimer()
    {
        Debug.Log("Return button clicked: Starting return timer.");

        isCountingUp = false;
        isCountingDown = true;
        isExploring = false;
        returnButton.gameObject.SetActive(false);

        explorationDialogue?.StopExploration();
        explorationManager?.StopExploring();

        timer = Mathf.Max(timer, 10f);

        DateTime now = DateTime.UtcNow;
        PlayerPrefs.SetString(RETURN_TIME_KEY, now.ToBinary().ToString());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, timer);
        PlayerPrefs.SetString(STATE_KEY, "returning");
        PlayerPrefs.Save();

        staminaBar.gameObject.SetActive(true);
        currentStamina = maxStamina;
        staminaBar.value = currentStamina;
    }

    public void SubtractOneHour()
    {
        if (!isCountingDown) return;

        timer -= 3600f;
        if (timer < 0f) timer = 0f;

        float returnDuration = PlayerPrefs.GetFloat(RETURN_DURATION_KEY, timer);
        DateTime now = DateTime.UtcNow;
        DateTime newReturnStart = now.AddSeconds(-(returnDuration - timer));

        PlayerPrefs.SetString(RETURN_TIME_KEY, newReturnStart.ToBinary().ToString());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, returnDuration);
        PlayerPrefs.Save();

        UpdateTimerDisplay(timer);
    }
}
