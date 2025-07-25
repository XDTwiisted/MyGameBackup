using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;  // New Input System

public class ExitBunkerManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Button exitBunkerButton;
    public Button returnButton;
    public Button subtractOneHourButton;
    public TextMeshProUGUI explorationTimerText;
    public Slider staminaBar;

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
        {
            subtractOneHourButton.onClick.AddListener(SubtractOneHour);
        }
    }

    public void StartExploration()
    {
        Debug.Log("StartExploration() called");

        character.SetActive(false);
        exitBunkerButton.gameObject.SetActive(false);
        returnButton.gameObject.SetActive(true);

        DateTime now = DateTime.UtcNow;
        PlayerPrefs.SetString(EXPLORE_TIME_KEY, now.ToBinary().ToString());
        PlayerPrefs.SetString(STATE_KEY, "exploring");
        PlayerPrefs.Save();

        timer = 0f;
        isCountingUp = true;
        isCountingDown = false;
        isExploring = true;

        staminaBar.gameObject.SetActive(true);
        currentStamina = maxStamina;
        staminaBar.value = currentStamina;

        if (explorationDialogue != null)
            explorationDialogue.StartExploration();

        if (explorationManager != null)
        {
            explorationManager.StartExploring();
        }
    }

    void Update()
    {
        HandleInput();

        if (isCountingUp)
        {
            float effectiveDelta = Time.deltaTime * CurrentSpeedMultiplier;
            timer += effectiveDelta;
            UpdateTimerDisplay(timer);

            if (explorationDialogue != null)
                explorationDialogue.UpdateDialogue(effectiveDelta);

            if (explorationManager != null)
                explorationManager.AdvanceExplorationTime(effectiveDelta);

            if (isHoldingScreen && currentStamina > 0f)
                currentStamina -= staminaDrainRate * Time.deltaTime;
            else
                currentStamina += staminaRegenRate * Time.deltaTime;

            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
            staminaBar.value = currentStamina;
        }
        else if (isCountingDown)
        {
            float effectiveDelta = Time.deltaTime * CurrentSpeedMultiplier;
            timer -= effectiveDelta;

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

                if (explorationDialogue != null)
                    explorationDialogue.ClearDialogue();

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
                TimeSpan timeGone = DateTime.UtcNow - exitTime;

                timer = (float)timeGone.TotalSeconds;
                isCountingUp = true;
                isCountingDown = false;
                isExploring = true;

                character.SetActive(false);
                exitBunkerButton.gameObject.SetActive(false);
                returnButton.gameObject.SetActive(true);

                if (explorationManager != null)
                    explorationManager.StartExploring();

                if (explorationDialogue != null)
                    explorationDialogue.StartExploration();
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

                    if (explorationDialogue != null)
                        explorationDialogue.ClearDialogue();
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

                    if (explorationDialogue != null)
                        explorationDialogue.StopExploration();

                    if (explorationManager != null)
                        explorationManager.StopExploring();
                }
            }
            else
            {
                timer = 0f;
                isCountingDown = false;

                character.SetActive(true);
                exitBunkerButton.gameObject.SetActive(true);
                returnButton.gameObject.SetActive(false);
                staminaBar.gameObject.SetActive(false);

                PlayerPrefs.SetString(STATE_KEY, "bunker");
                PlayerPrefs.Save();
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

            if (explorationDialogue != null)
                explorationDialogue.ClearDialogue();
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

        if (explorationDialogue != null)
            explorationDialogue.StopExploration();

        if (explorationManager != null)
            explorationManager.StopExploring();

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
        if (!isCountingDown)
        {
            Debug.Log("SubtractOneHour ignored: Not currently returning.");
            return;
        }

        timer -= 3600f;
        if (timer < 0f)
            timer = 0f;

        Debug.Log($"Return timer reduced by 1 hour. New timer: {timer}");

        float returnDuration = PlayerPrefs.GetFloat(RETURN_DURATION_KEY, timer);
        DateTime now = DateTime.UtcNow;
        DateTime newReturnStart = now.AddSeconds(-(returnDuration - timer));

        PlayerPrefs.SetString(RETURN_TIME_KEY, newReturnStart.ToBinary().ToString());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, returnDuration);
        PlayerPrefs.Save();

        UpdateTimerDisplay(timer);
    }
}
