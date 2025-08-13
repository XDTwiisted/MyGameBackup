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
    public GameObject character; // Player root with child SpriteRenderers (Walking, Shadow1, Shadow2)

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
    private float timer = 0f;              // counts up during explore, down during return
    private bool isCountingUp = false;
    private bool isCountingDown = false;
    private bool isHoldingScreen = false;
    private bool staminaLocked = false;
    private bool staminaMustFullyRecharge = false;

    private GameObject backgroundGroup;
    private ScrollingBackground[] backgroundScrollers;
    private ScrollingForeground[] foregroundScrollers;

    // Cache all SpriteRenderers on character so we can flip them
    private SpriteRenderer[] characterSprites;

    private enum GameState { Bunker, Exploring, Returning }

    private const string STATE_KEY = "gameState";
    private const string EXPLORE_TIME_KEY = "explorationStartTime"; // DateTime.UtcNow.ToBinary()
    private const string RETURN_TIME_KEY = "returnStartTime";       // DateTime.UtcNow.ToBinary()
    private const string RETURN_DURATION_KEY = "returnDuration";    // float seconds

    public float CurrentSpeedMultiplier => (isHoldingScreen && !staminaLocked) ? speedMultiplier : 1f;

    private static class TimeUtil
    {
        public static DateTime UtcNow() => DateTime.UtcNow;

        public static void SaveUtcBinary(string key, DateTime utc)
        {
            PlayerPrefs.SetString(key, utc.ToBinary().ToString());
        }

        public static bool TryLoadUtcBinary(string key, out DateTime utc)
        {
            utc = default;
            var s = PlayerPrefs.GetString(key, string.Empty);
            if (string.IsNullOrEmpty(s)) return false;
            if (!long.TryParse(s, out var bin)) return false;
            utc = DateTime.FromBinary(bin);
            return true;
        }
    }

    void Start()
    {
        if (returnButton != null) returnButton.gameObject.SetActive(false);
        if (explorationTimerText != null) explorationTimerText.text = "Time Outside: 00:00:00";

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = maxStamina;
            staminaBar.gameObject.SetActive(false); // hidden while exploring; shown while returning
        }

        if (explorationDialogue != null && explorationDialogue.dialogueText != null)
            explorationDialogue.dialogueText.enabled = false;

        // Cache character sprite renderers for flipping
        if (character != null)
            characterSprites = character.GetComponentsInChildren<SpriteRenderer>(true);

        backgroundGroup = GameObject.Find("Background");
        if (backgroundGroup != null)
        {
            backgroundGroup.SetActive(false);
            backgroundScrollers = backgroundGroup.GetComponentsInChildren<ScrollingBackground>(true);
            foregroundScrollers = backgroundGroup.GetComponentsInChildren<ScrollingForeground>(true);
        }

        if (returnButton != null) returnButton.onClick.AddListener(StartReturnTimer);
        if (subtractOneHourButton != null) subtractOneHourButton.onClick.AddListener(SubtractOneHour);

        if (exitBunkerButton != null)
        {
            exitBunkerButton.onClick.RemoveAllListeners();
            exitBunkerButton.onClick.AddListener(ToggleGearUpPanel);
        }

        if (stashButton != null)
        {
            var b = stashButton.GetComponent<Button>();
            if (b != null)
            {
                b.onClick.RemoveAllListeners();
                b.onClick.AddListener(ToggleStashPanel);
            }
        }

        if (confirmExplorationButton != null)
        {
            confirmExplorationButton.onClick.RemoveAllListeners();
            confirmExplorationButton.onClick.AddListener(OnConfirmExploration);
        }

        if (cancelExplorationButton != null)
        {
            cancelExplorationButton.onClick.RemoveAllListeners();
            cancelExplorationButton.onClick.AddListener(() =>
            {
                if (gearUpPanel != null) gearUpPanel.SetActive(false);
            });
        }

        if (gearUpPanel != null) gearUpPanel.SetActive(false);

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

            // Persist exploration start time by inferring from current elapsed
            DateTime simulatedStartUtc = TimeUtil.UtcNow() - TimeSpan.FromSeconds(timer);
            TimeUtil.SaveUtcBinary(EXPLORE_TIME_KEY, simulatedStartUtc);
            PlayerPrefs.Save();

            if (explorationDialogue != null) explorationDialogue.UpdateDialogue(delta);
            if (explorationManager != null) explorationManager.AdvanceExplorationTime(delta);
        }

        if (isCountingDown)
        {
            float delta = Time.deltaTime * CurrentSpeedMultiplier;
            timer -= delta;
            if (timer < 0f) timer = 0f;
            UpdateTimerDisplay(timer);

            if (timer <= 0f)
            {
                isCountingDown = false;
                FinishReturn();
            }
        }

        // Stamina only during return
        if ((isCountingUp || isCountingDown) && staminaBar != null && staminaBar.gameObject.activeSelf)
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

        // Do not process hold if inventory panel is open
        if (inventoryPanel != null && inventoryPanel.activeSelf) return;

        float screenHeight = Screen.height;
        float minY = screenHeight * 0.20f;
        float maxY = screenHeight * 0.80f;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            float y = Touchscreen.current.primaryTouch.position.ReadValue().y;
            if (y > minY && y < maxY) isHoldingScreen = true;
        }
        else if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            float y = Mouse.current.position.ReadValue().y;
            if (y > minY && y < maxY) isHoldingScreen = true;
        }
    }

    private void ToggleGearUpPanel()
    {
        if (gearUpPanel == null) return;
        gearUpPanel.SetActive(!gearUpPanel.activeSelf);
        if (gearUpPanel.activeSelf)
        {
            if (stashPanel != null) stashPanel.SetActive(false);
            if (inventoryPanel != null) inventoryPanel.SetActive(false);
        }
    }

    private void ToggleStashPanel()
    {
        if (stashPanel == null) return;

        // Flip only the stash panel
        bool show = !stashPanel.activeSelf;
        stashPanel.SetActive(show);

        // Optional: if you NEVER want both visible at the same time,
        // close inventory when opening stash (but do NOT open it when closing stash)
        if (show && inventoryPanel != null)
            inventoryPanel.SetActive(false);

        // Do not touch gearUpPanel here
    }

    private void OnConfirmExploration()
    {
        // 1) Move gear-up selections into the live inventory
        var gsm = FindFirstObjectByType<GearUpSelectionManager>();
        var inv = InventoryManager.Instance;

        if (gsm != null && inv != null)
        {
            // Stackables (Food/Health/etc.)
            var stackables = gsm.GetStackables(); // Dictionary<InventoryItemData, int>
            if (stackables != null)
            {
                foreach (var kv in stackables)
                {
                    var data = kv.Key;
                    var qty = kv.Value;
                    if (data != null && qty > 0)
                        inv.AddItem(data, qty); // uses your existing method
                }
            }

            // Durables (weapons/tools) as instances
            var durables = gsm.GetDurables(); // List<ItemInstance>
            if (durables != null)
            {
                foreach (var inst in durables)
                {
                    if (inst != null && inst.itemData != null)
                        inv.AddItemInstance(inst); // uses your existing method
                }
            }

            // Clear gear-up selections after transfer
            gsm.ClearSelections();
        }

        // 2) Save start time and state in UTC
        TimeUtil.SaveUtcBinary(EXPLORE_TIME_KEY, TimeUtil.UtcNow());
        PlayerPrefs.SetString(STATE_KEY, GameState.Exploring.ToString());
        PlayerPrefs.Save();

        // 3) UI and systems
        SetExplorationUIState(true);
        StartExplorationInternal();

        // >>> FIX: actually start ExplorationManager so loot can tick <<<
        if (explorationManager != null)
            explorationManager.StartExploring();

        if (backgroundGroup != null) backgroundGroup.SetActive(true);
        SetBackgroundScrolling(true, false);

        SetCharacterFacingRight(true);
    }

    private void StartExplorationInternal()
    {
        if (!TimeUtil.TryLoadUtcBinary(EXPLORE_TIME_KEY, out var startUtc))
        {
            startUtc = TimeUtil.UtcNow();
            TimeUtil.SaveUtcBinary(EXPLORE_TIME_KEY, startUtc);
            PlayerPrefs.Save();
        }

        double elapsed = (TimeUtil.UtcNow() - startUtc).TotalSeconds;
        timer = Mathf.Max(0f, (float)elapsed);
        isCountingUp = true;
        isCountingDown = false;

        if (explorationDialogue != null && explorationDialogue.dialogueText != null)
            explorationDialogue.dialogueText.enabled = true;

        if (returnButton != null) returnButton.gameObject.SetActive(true);
        if (staminaBar != null) staminaBar.gameObject.SetActive(true); // show while exploring

        SetCharacterFacingRight(true);
    }

    private void StartReturnTimer()
    {
        // Compute return duration based on how long outside (timer)
        float duration = Mathf.Max(1f, timer); // simple 1:1 mapping (tune to taste)
        timer = duration;
        isCountingUp = false;
        isCountingDown = true;

        // Save return start and duration in UTC
        TimeUtil.SaveUtcBinary(RETURN_TIME_KEY, TimeUtil.UtcNow());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, duration);
        PlayerPrefs.SetString(STATE_KEY, GameState.Returning.ToString());
        PlayerPrefs.Save();

        // UI
        if (staminaBar != null)
        {
            staminaBar.gameObject.SetActive(true); // visible while returning
            currentStamina = maxStamina;
            staminaLocked = false;
            staminaMustFullyRecharge = false;
            staminaBar.value = currentStamina;
        }

        // Pause loot generation without hiding the dialogue
        if (explorationManager != null)
            explorationManager.enabled = false;

        // Scroll reverse
        if (backgroundGroup != null) backgroundGroup.SetActive(true);
        SetBackgroundScrolling(true, true);

        // Keep exploration dialogue visible (do NOT disable here)

        // Face left while returning
        SetCharacterFacingRight(false);
    }



    private void SubtractOneHour()
    {
        // Allows shaving 1 hour off elapsed exploration or remaining return (design tool)
        if (isCountingUp)
        {
            timer = Mathf.Max(0f, timer - 3600f);
            UpdateTimerDisplay(timer);

            // Shift saved start time forward by 1h so restore remains consistent
            if (TimeUtil.TryLoadUtcBinary(EXPLORE_TIME_KEY, out var startUtc))
            {
                startUtc = startUtc.AddHours(1);
                TimeUtil.SaveUtcBinary(EXPLORE_TIME_KEY, startUtc);
                PlayerPrefs.Save();
            }
        }
        else if (isCountingDown)
        {
            // Or reduce remaining return time
            timer = Mathf.Max(0f, timer - 3600f);
            UpdateTimerDisplay(timer);

            // Adjust persisted duration so resume is consistent
            float savedDuration = PlayerPrefs.GetFloat(RETURN_DURATION_KEY, timer);
            savedDuration = Mathf.Max(0f, savedDuration - 3600f);
            PlayerPrefs.SetFloat(RETURN_DURATION_KEY, savedDuration);
            PlayerPrefs.Save();

            if (timer <= 0f)
            {
                isCountingDown = false;
                FinishReturn();
            }
        }
    }

    private void UpdateTimerDisplay(float seconds)
    {
        if (explorationTimerText == null) return;
        TimeSpan ts = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        string hms = string.Format("{0:00}:{1:00}:{2:00}", (int)ts.TotalHours, ts.Minutes, ts.Seconds);
        if (isCountingDown)
            explorationTimerText.text = "Returning: " + hms;
        else if (isCountingUp)
            explorationTimerText.text = "Time Outside: " + hms;
        else
            explorationTimerText.text = "Time Outside: 00:00:00";
    }

    private void FinishReturn()
    {
        // Clear timers/state
        isCountingUp = false;
        isCountingDown = false;
        timer = 0f;

        // Transfer loot to stash and clear inventory
        var inv = InventoryManager.Instance;
        var stash = StashManager.Instance;
        if (inv != null && stash != null)
        {
            var (stackables, durables) = inv.GetAllItems(); // tuple destructuring
            stash.AddItems(stackables, durables);
            inv.ClearAllItems();
        }

        // Re-enable ExplorationManager now that we're back
        if (explorationManager != null)
        {
            explorationManager.enabled = true;
            explorationManager.ReturnToBunker();
        }

        // UI
        SetBackgroundScrolling(false, false);
        if (backgroundGroup != null) backgroundGroup.SetActive(false);
        if (staminaBar != null) staminaBar.gameObject.SetActive(false);
        if (returnButton != null) returnButton.gameObject.SetActive(false);

        // Now hide the exploration dialogue text after returning
        if (explorationDialogue != null && explorationDialogue.dialogueText != null)
            explorationDialogue.dialogueText.enabled = false;

        SetBunkerUIState();

        // Face right by default in bunker
        SetCharacterFacingRight(true);

        // Save bunker state
        PlayerPrefs.DeleteKey(EXPLORE_TIME_KEY);
        PlayerPrefs.DeleteKey(RETURN_TIME_KEY);
        PlayerPrefs.DeleteKey(RETURN_DURATION_KEY);
        PlayerPrefs.SetString(STATE_KEY, GameState.Bunker.ToString());
        PlayerPrefs.Save();

        UpdateTimerDisplay(0f);
    }


    private void SetExplorationUIState(bool exploring)
    {
        // Buttons
        if (returnButton != null) returnButton.gameObject.SetActive(exploring);
        if (exitBunkerButton != null) exitBunkerButton.gameObject.SetActive(!exploring);

        // Panels
        if (gearUpPanel != null) gearUpPanel.SetActive(false);
        if (stashPanel != null) stashPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);

        // Top-level toggles
        if (stashButton != null) stashButton.SetActive(false);
        if (inventoryButton != null) inventoryButton.SetActive(true);
    }

    private void SetBunkerUIState()
    {
        if (exitBunkerButton != null) exitBunkerButton.gameObject.SetActive(true);
        if (stashButton != null) stashButton.SetActive(true);
        if (inventoryButton != null) inventoryButton.SetActive(false);
        if (gearUpPanel != null) gearUpPanel.SetActive(false);
        if (stashPanel != null) stashPanel.SetActive(false);
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
    }

    private void SetBackgroundScrolling(bool enable, bool reverseDirection)
    {
        if (backgroundScrollers != null)
        {
            foreach (var s in backgroundScrollers)
            {
                if (s == null) continue;
                s.isScrolling = enable;
                s.reverseDirection = reverseDirection;
            }
        }
        if (foregroundScrollers != null)
        {
            foreach (var s in foregroundScrollers)
            {
                if (s == null) continue;
                s.isScrolling = enable;
                s.reverseDirection = reverseDirection;
            }
        }
    }

    private void RestoreState()
    {
        string stateStr = PlayerPrefs.GetString(STATE_KEY, GameState.Bunker.ToString());
        GameState state;
        if (!Enum.TryParse(stateStr, out state)) state = GameState.Bunker;

        switch (state)
        {
            case GameState.Exploring:
                {
                    if (!TimeUtil.TryLoadUtcBinary(EXPLORE_TIME_KEY, out var startUtc))
                    {
                        SetBunkerUIState();
                        SetCharacterFacingRight(true);
                        PlayerPrefs.SetString(STATE_KEY, GameState.Bunker.ToString());
                        PlayerPrefs.Save();
                        return;
                    }

                    SetExplorationUIState(true);

                    double elapsed = (TimeUtil.UtcNow() - startUtc).TotalSeconds;
                    timer = Mathf.Max(0f, (float)elapsed);
                    isCountingUp = true;
                    isCountingDown = false;

                    if (backgroundGroup != null) backgroundGroup.SetActive(true);
                    SetBackgroundScrolling(true, false);

                    // >>> FIX: ensure ExplorationManager is running after restore <<<
                    if (explorationManager != null)
                        explorationManager.StartExploring();

                    if (explorationDialogue != null && explorationDialogue.dialogueText != null)
                        explorationDialogue.dialogueText.enabled = true;

                    if (staminaBar != null) staminaBar.gameObject.SetActive(true); // show while exploring

                    SetCharacterFacingRight(true);
                    UpdateTimerDisplay(timer);
                    break;
                }

            case GameState.Returning:
                {
                    bool haveStart = TimeUtil.TryLoadUtcBinary(RETURN_TIME_KEY, out var retStartUtc);
                    float duration = PlayerPrefs.GetFloat(RETURN_DURATION_KEY, 0f);
                    if (!haveStart || duration <= 0f)
                    {
                        // Not enough info, finish immediately to avoid stuck states
                        FinishReturn();
                        return;
                    }

                    double elapsed = (TimeUtil.UtcNow() - retStartUtc).TotalSeconds;
                    float remaining = Mathf.Max(0f, duration - (float)elapsed);

                    if (remaining <= 0f)
                    {
                        FinishReturn();
                        return;
                    }

                    // Resume returning
                    timer = remaining;
                    isCountingUp = false;
                    isCountingDown = true;

                    // Pause loot generation while returning
                    if (explorationManager != null)
                        explorationManager.enabled = false;

                    if (backgroundGroup != null) backgroundGroup.SetActive(true);
                    SetBackgroundScrolling(true, true);

                    if (staminaBar != null)
                    {
                        staminaBar.gameObject.SetActive(true);
                        currentStamina = maxStamina;
                        staminaLocked = false;
                        staminaMustFullyRecharge = false;
                        staminaBar.value = currentStamina;
                    }

                    // Keep exploration dialogue visible (do NOT disable here)

                    if (returnButton != null) returnButton.gameObject.SetActive(true);
                    if (exitBunkerButton != null) exitBunkerButton.gameObject.SetActive(false);
                    if (stashButton != null) stashButton.SetActive(false);
                    if (inventoryButton != null) inventoryButton.SetActive(true);

                    // Face left while returning
                    SetCharacterFacingRight(false);

                    UpdateTimerDisplay(timer);
                    break;
                }


            default:
            case GameState.Bunker:
                {
                    SetBunkerUIState();
                    SetCharacterFacingRight(true);
                    UpdateTimerDisplay(0f);
                    break;
                }
        }
    }

    private void SetCharacterFacingRight(bool facingRight)
    {
        if (characterSprites == null) return;
        // Flip each child sprite; right-facing = flipX false; left-facing = flipX true
        bool flip = !facingRight;
        for (int i = 0; i < characterSprites.Length; i++)
        {
            if (characterSprites[i] == null) continue;
            characterSprites[i].flipX = flip;
        }
    }
}
