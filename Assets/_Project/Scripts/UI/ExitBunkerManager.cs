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

    private GameObject backgroundGroup;
    private ScrollingBackground[] backgroundScrollers;
    private ScrollingForeground[] foregroundScrollers;

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

        backgroundGroup = GameObject.Find("Background");
        if (backgroundGroup != null)
        {
            backgroundGroup.SetActive(false);
            backgroundScrollers = backgroundGroup.GetComponentsInChildren<ScrollingBackground>(true);
            foregroundScrollers = backgroundGroup.GetComponentsInChildren<ScrollingForeground>(true);
        }

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

            if (backgroundGroup != null) backgroundGroup.SetActive(true);
            SetBackgroundScrolling(true, false);
        });

        cancelExplorationButton?.onClick.RemoveAllListeners();
        cancelExplorationButton?.onClick.AddListener(() => gearUpPanel.SetActive(false));

        gearUpPanel?.SetActive(false);

        RestoreState();
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
            UpdateTimerDisplay(timer);

            if (timer <= 0f)
            {
                timer = 0f;
                isCountingDown = false;
                FinishReturn();
            }
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
        gearUpPanel.SetActive(!gearUpPanel.activeSelf);
    }

    private void ToggleStashPanel()
    {
        bool isOpening = !stashPanel.activeSelf;

        stashPanel.SetActive(isOpening);
        inventoryPanel.SetActive(false);
        gearUpPanel?.SetActive(false);

        if (isOpening)
        {
            StashManagerUI.Instance?.RefreshStashUI();
        }
    }

    private void StartExplorationInternal()
    {
        isCountingUp = true;
        isExploring = true;
        timer = 0f;

        returnButton.gameObject.SetActive(true);
        exitBunkerButton.gameObject.SetActive(false);
        inventoryButton.SetActive(true);
        stashButton.SetActive(false);
        stashPanel.SetActive(false);
        gearUpPanel.SetActive(false);
        staminaBar.gameObject.SetActive(true);

        explorationManager?.StartExploring();
        FlipCharacter(false);
    }

    private void StartReturnTimer()
    {
        if (!isCountingUp) return;

        isCountingUp = false;
        isCountingDown = true;

        float duration = timer;
        PlayerPrefs.SetString(RETURN_TIME_KEY, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, duration);
        PlayerPrefs.SetString(STATE_KEY, "returning");
        PlayerPrefs.Save();

        timer = duration;

        SetBackgroundScrolling(true, true);
        FlipCharacter(true);
        staminaBar.gameObject.SetActive(true);
    }

    private void FinishReturn()
    {
        isCountingDown = false;
        isExploring = false;

        SetExplorationUIState(false);
        staminaBar.gameObject.SetActive(false);

        PlayerPrefs.DeleteKey(EXPLORE_TIME_KEY);
        PlayerPrefs.DeleteKey(RETURN_TIME_KEY);
        PlayerPrefs.DeleteKey(RETURN_DURATION_KEY);
        PlayerPrefs.SetString(STATE_KEY, "bunker");
        PlayerPrefs.Save();

        if (StashManager.Instance != null && InventoryManager.Instance != null)
        {
            var (stackables, durables) = InventoryManager.Instance.GetAllItems();
            StashManager.Instance.AddItems(stackables, durables);
            InventoryManager.Instance.ClearAllItems();
        }

        explorationManager?.ReturnToBunker();

        if (backgroundGroup != null)
            backgroundGroup.SetActive(false);

        if (character != null)
        {
            foreach (var sr in character.GetComponentsInChildren<SpriteRenderer>())
                sr.flipX = false;
        }
    }

    private void SetBackgroundScrolling(bool scroll, bool reverse)
    {
        if (backgroundScrollers != null)
        {
            foreach (var scroller in backgroundScrollers)
            {
                scroller.isScrolling = scroll;
                scroller.reverseDirection = reverse;
            }
        }

        if (foregroundScrollers != null)
        {
            foreach (var scroller in foregroundScrollers)
            {
                scroller.isScrolling = scroll;
                scroller.reverseDirection = reverse;
            }
        }
    }

    private void SetExplorationUIState(bool exploring)
    {
        exitBunkerButton.gameObject.SetActive(!exploring);
        returnButton.gameObject.SetActive(exploring);
        inventoryButton.SetActive(exploring);
        stashButton.SetActive(!exploring);
    }

    private void SubtractOneHour()
    {
        if (isCountingDown && timer >= 3600f)
        {
            timer -= 3600f;
        }
    }

    private void UpdateTimerDisplay(float timeInSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(timeInSeconds);
        explorationTimerText.text = "Time Outside: " + time.ToString(@"hh\:mm\:ss");
    }

    private void RestoreState()
    {
        string state = PlayerPrefs.GetString(STATE_KEY, "bunker");
        Debug.Log("[RestoreState] Loaded state: " + state);

        if (state == "exploring")
        {
            SetExplorationUIState(true);
            isCountingUp = true;
            isExploring = true;

            if (PlayerPrefs.HasKey(EXPLORE_TIME_KEY))
            {
                long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(EXPLORE_TIME_KEY));
                DateTime startTime = DateTime.FromBinary(binaryTime);
                timer = (float)(DateTime.UtcNow - startTime).TotalSeconds;

                Debug.Log("[RestoreState] Exploring: Recovered timer = " + timer);
            }

            if (backgroundGroup != null) backgroundGroup.SetActive(true);
            SetBackgroundScrolling(true, false);
            returnButton.gameObject.SetActive(true);
            staminaBar.gameObject.SetActive(true);
            FlipCharacter(false);
        }
        else if (state == "returning")
        {
            if (PlayerPrefs.HasKey(RETURN_TIME_KEY) && PlayerPrefs.HasKey(RETURN_DURATION_KEY))
            {
                long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(RETURN_TIME_KEY));
                float savedDuration = PlayerPrefs.GetFloat(RETURN_DURATION_KEY);

                DateTime returnStart = DateTime.FromBinary(binaryTime);
                float timePassed = (float)(DateTime.UtcNow - returnStart).TotalSeconds;
                float timeLeft = savedDuration - timePassed;

                Debug.Log("[RestoreState] Returning: duration=" + savedDuration + ", timePassed=" + timePassed + ", timeLeft=" + timeLeft);

                if (timeLeft <= 0f)
                {
                    FinishReturn();
                }
                else
                {
                    timer = timeLeft;
                    isCountingDown = true;

                    if (backgroundGroup != null) backgroundGroup.SetActive(true);
                    SetBackgroundScrolling(true, true);
                    staminaBar.gameObject.SetActive(false);
                    FlipCharacter(true);
                }
            }
            else
            {
                FinishReturn();
            }
        }
        else
        {
            SetExplorationUIState(false);
            timer = 0f;
            FlipCharacter(false);
        }
    }

    private void FlipCharacter(bool faceLeft)
    {
        if (character == null) return;

        Transform walking = character.transform.Find("Walking");
        Transform shadow1 = character.transform.Find("Shadow1");
        Transform shadow2 = character.transform.Find("Shadow2");

        bool flip = faceLeft;

        if (walking?.GetComponent<SpriteRenderer>() != null)
            walking.GetComponent<SpriteRenderer>().flipX = flip;

        if (shadow1?.GetComponent<SpriteRenderer>() != null)
            shadow1.GetComponent<SpriteRenderer>().flipX = flip;

        if (shadow2?.GetComponent<SpriteRenderer>() != null)
            shadow2.GetComponent<SpriteRenderer>().flipX = flip;
    }
}
