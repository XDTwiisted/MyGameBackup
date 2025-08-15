using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;

/// <summary>
/// ExitBunkerManager
/// - Handles bunker/exploring/returning state transitions
/// - Manages UI panels (Gear Up, Stash, Inventory) and visibility rules
/// - Tracks exploration (count-up) and return (count-down) timers with real-time persistence
/// - Supports stamina-based speed-up while returning
/// - Fix: Back/Cancel from Gear Up returns to bunker with STASH VISIBLE
/// - Fix: Opening Gear Up does NOT hide the stash
/// </summary>
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

    // Background group + cached scrollers (optional)
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

    // Optional: cache original scroll speeds if scripts expose "scrollSpeed"
    private float[] bgOriginalSpeeds;
    private float[] fgOriginalSpeeds;

    public float CurrentSpeedMultiplier => (isHoldingScreen && !staminaLocked && !staminaMustFullyRecharge) ? speedMultiplier : 1f;

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

    private void Start()
    {
        if (returnButton != null) returnButton.gameObject.SetActive(false);
        if (explorationTimerText != null) explorationTimerText.text = "Time Outside: 00:00:00";

        currentStamina = maxStamina;
        if (staminaBar != null)
        {
            staminaBar.maxValue = maxStamina;
            staminaBar.value = maxStamina;
            staminaBar.gameObject.SetActive(false); // hidden in bunker/exploring; shown during returning
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

            // Cache original speeds if available via a public "scrollSpeed" field
            if (backgroundScrollers != null)
            {
                bgOriginalSpeeds = new float[backgroundScrollers.Length];
                for (int i = 0; i < backgroundScrollers.Length; i++)
                {
                    var f = backgroundScrollers[i].GetType().GetField("scrollSpeed");
                    bgOriginalSpeeds[i] = (f != null) ? (float)f.GetValue(backgroundScrollers[i]) : 0f;
                }
            }
            if (foregroundScrollers != null)
            {
                fgOriginalSpeeds = new float[foregroundScrollers.Length];
                for (int i = 0; i < foregroundScrollers.Length; i++)
                {
                    var f = foregroundScrollers[i].GetType().GetField("scrollSpeed");
                    fgOriginalSpeeds[i] = (f != null) ? (float)f.GetValue(foregroundScrollers[i]) : 0f;
                }
            }
        }

        // Wire buttons
        if (returnButton != null)
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
            // Back/Cancel from Gear Up -> return to bunker with STASH VISIBLE
            cancelExplorationButton.onClick.AddListener(ShowBunkerUI);
        }

        if (gearUpPanel != null) gearUpPanel.SetActive(false);

        RestoreState();

        // If nothing restored us into exploring/returning, ensure proper bunker visibility
        EnsureBunkerVisibilityIfIdle();
    }

    private void Update()
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

        // Stamina only when the bar is visible (returning)
        if (staminaBar != null && staminaBar.gameObject.activeSelf)
        {
            if (isHoldingScreen && currentStamina > 0f && !staminaMustFullyRecharge)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                if (currentStamina <= 0f)
                {
                    currentStamina = 0f;
                    staminaLocked = true; // must release to recharge
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
            staminaBar.value = currentStamina;
        }
    }

    // ---------- INPUT ----------
    private void HandleInput()
    {
        // Pointer/Touch hold to speed-up
        bool pointerDown = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool touchDown = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
        isHoldingScreen = pointerDown || touchDown;
        if (!isHoldingScreen)
        {
            // Releasing the hold unlocks recharge logic
            if (staminaLocked) staminaMustFullyRecharge = true;
        }
    }

    // ---------- UI HELPERS (Visibility Rules) ----------

    /// <summary>
    /// Show bunker UI with stash visible, inventory hidden, gear-up hidden.
    /// </summary>
    // Show bunker UI with stash visible; Exit Bunker button ON, Return button OFF
    private void ShowBunkerUI()
    {
        if (stashButton) stashButton.SetActive(true);
        if (stashPanel) stashPanel.SetActive(true);

        if (inventoryButton) inventoryButton.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);

        if (gearUpPanel) gearUpPanel.SetActive(false);

        if (staminaBar) staminaBar.gameObject.SetActive(false);

        if (explorationDialogue && explorationDialogue.dialogueText)
            explorationDialogue.dialogueText.enabled = false;

        if (backgroundGroup) backgroundGroup.SetActive(false);

        // button visibility
        if (exitBunkerButton) exitBunkerButton.gameObject.SetActive(true);
        if (returnButton) returnButton.gameObject.SetActive(false);

        PlayerPrefs.SetString(STATE_KEY, GameState.Bunker.ToString());
        PlayerPrefs.Save();
    }


    /// <summary>
    /// UI when exploring: stash hidden, inventory button visible (panel optional), dialogue on, background scrolling forward.
    /// </summary>
    // Exploring: Exit Bunker button OFF, Return button ON
    private void ShowExploringUI()
    {
        if (stashButton) stashButton.SetActive(false);
        if (stashPanel) stashPanel.SetActive(false);

        if (inventoryButton) inventoryButton.SetActive(true);
        if (inventoryPanel) inventoryPanel.SetActive(false);

        if (gearUpPanel) gearUpPanel.SetActive(false);

        if (staminaBar) staminaBar.gameObject.SetActive(false);

        if (explorationDialogue && explorationDialogue.dialogueText)
            explorationDialogue.dialogueText.enabled = true;

        if (backgroundGroup) backgroundGroup.SetActive(true);
        SetBackgroundScrolling(forward: true);

        // button visibility
        if (exitBunkerButton) exitBunkerButton.gameObject.SetActive(false);
        if (returnButton) returnButton.gameObject.SetActive(true);
    }


    /// <summary>
    /// UI when returning: stash/inventory hidden, stamina visible, background scrolling reverse.
    /// </summary>
    // Returning: both buttons OFF (countdown is running)
    private void ShowReturningUI()
    {
        if (stashButton) stashButton.SetActive(false);
        if (stashPanel) stashPanel.SetActive(false);

        if (inventoryButton) inventoryButton.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);

        if (gearUpPanel) gearUpPanel.SetActive(false);

        if (staminaBar)
        {
            staminaBar.gameObject.SetActive(true);
            currentStamina = maxStamina;
            staminaLocked = false;
            staminaMustFullyRecharge = false;
            staminaBar.value = currentStamina;
        }

        if (explorationDialogue && explorationDialogue.dialogueText)
            explorationDialogue.dialogueText.enabled = false;

        if (backgroundGroup) backgroundGroup.SetActive(true);
        SetBackgroundScrolling(forward: false);

        //  button visibility
        if (exitBunkerButton) exitBunkerButton.gameObject.SetActive(false);
        if (returnButton) returnButton.gameObject.SetActive(false);
    }


    private void EnsureBunkerVisibilityIfIdle()
    {
        string s = PlayerPrefs.GetString(STATE_KEY, GameState.Bunker.ToString());
        if (s == GameState.Bunker.ToString() && !isCountingUp && !isCountingDown)
        {
            ShowBunkerUI();
        }
    }

    // ---------- BUTTON CALLBACKS ----------

    // Exit bunker button (opens/closes Gear Up). DOES NOT hide the stash.
    private void ToggleGearUpPanel()
    {
        if (!gearUpPanel) return;

        bool show = !gearUpPanel.activeSelf;
        gearUpPanel.SetActive(show);

        // Keep stash visible in bunker
        if (stashButton) stashButton.SetActive(true);
        if (stashPanel) stashPanel.SetActive(true);

        // Inventory stays hidden while in bunker
        if (inventoryButton) inventoryButton.SetActive(false);
        if (inventoryPanel) inventoryPanel.SetActive(false);
    }

    // Stash button only toggles the stash panel, not the button itself
    private void ToggleStashPanel()
    {
        if (!stashPanel) return;
        stashPanel.SetActive(!stashPanel.activeSelf);
    }

    // Confirm exploration -> start exploring
    private void OnConfirmExploration()
    {
        if (gearUpPanel) gearUpPanel.SetActive(false);

        FlipCharacter(faceLeft: false);

        timer = 0f;
        isCountingUp = true;
        isCountingDown = false;

        TimeUtil.SaveUtcBinary(EXPLORE_TIME_KEY, TimeUtil.UtcNow());
        PlayerPrefs.SetString(STATE_KEY, GameState.Exploring.ToString());
        PlayerPrefs.Save();

        ShowExploringUI();

        //  add this:
        if (explorationManager != null) explorationManager.StartExploring();
    }


    private void SubtractOneHour()
    {
        if (!isCountingUp) return;
        timer = Mathf.Max(0f, timer - 3600f);
        UpdateTimerDisplay(timer);

        // Recompute and persist simulated start
        DateTime simulatedStartUtc = TimeUtil.UtcNow() - TimeSpan.FromSeconds(timer);
        TimeUtil.SaveUtcBinary(EXPLORE_TIME_KEY, simulatedStartUtc);
        PlayerPrefs.Save();
    }

    // Start the return (count-down) based on time spent outside
    private void StartReturnTimer()
    {
        if (isCountingDown) return;

        // Compute return duration based on time outside (tune as desired)
        float duration = Mathf.Max(1f, timer); // simple 1:1 mapping now
        timer = duration;
        isCountingUp = false;
        isCountingDown = true;

        // Save return start and duration in UTC
        TimeUtil.SaveUtcBinary(RETURN_TIME_KEY, TimeUtil.UtcNow());
        PlayerPrefs.SetFloat(RETURN_DURATION_KEY, duration);
        PlayerPrefs.SetString(STATE_KEY, GameState.Returning.ToString());
        PlayerPrefs.Save();

        // Flip character to face left when returning
        FlipCharacter(faceLeft: true);

        // UI
        ShowReturningUI();

        if (explorationManager != null) explorationManager.StopExploring();

    }

    // Called when countdown reaches 0 or return finished offline
    private void FinishReturn()
    {
        // Transfer loot to stash happens elsewhere in your codebase; keep this method focused on state/UI.

        // Reset timers/flags
        timer = 0f;
        isCountingUp = false;
        isCountingDown = false;

        // Clear persisted return keys
        PlayerPrefs.DeleteKey(RETURN_TIME_KEY);
        PlayerPrefs.DeleteKey(RETURN_DURATION_KEY);

        // Back to bunker state
        PlayerPrefs.SetString(STATE_KEY, GameState.Bunker.ToString());
        PlayerPrefs.Save();

        // Ensure character faces right again in bunker
        FlipCharacter(faceLeft: false);

        // UI
        ShowBunkerUI();

        //  add this:
        if (explorationManager != null) explorationManager.StopExploring();

    }

    // ---------- RESTORE ----------

    private void RestoreState()
    {
        string stateStr = PlayerPrefs.GetString(STATE_KEY, GameState.Bunker.ToString());

        if (stateStr == GameState.Exploring.ToString())
        {
            // Restore exploring: compute elapsed since EXPLORE_TIME_KEY
            if (TimeUtil.TryLoadUtcBinary(EXPLORE_TIME_KEY, out var startUtc))
            {
                var elapsed = (float)(TimeUtil.UtcNow() - startUtc).TotalSeconds;
                timer = Mathf.Max(0f, elapsed);
                isCountingUp = true;
                isCountingDown = false;

                // Face right going out
                FlipCharacter(faceLeft: false);

                ShowExploringUI();
                UpdateTimerDisplay(timer);
            }
            else
            {
                // Fallback
                ShowBunkerUI();
            }
        }
        else if (stateStr == GameState.Returning.ToString())
        {
            // Restore returning: compute remaining based on RETURN_TIME + RETURN_DURATION
            if (TimeUtil.TryLoadUtcBinary(RETURN_TIME_KEY, out var returnStart))
            {
                float duration = PlayerPrefs.GetFloat(RETURN_DURATION_KEY, 0f);
                var elapsed = (float)(TimeUtil.UtcNow() - returnStart).TotalSeconds;
                float remaining = Mathf.Max(0f, duration - elapsed);

                if (remaining <= 0f)
                {
                    // Already finished while offline
                    FinishReturn();
                    return;
                }

                timer = remaining;
                isCountingUp = false;
                isCountingDown = true;

                // Face left while returning
                FlipCharacter(faceLeft: true);

                ShowReturningUI();
                UpdateTimerDisplay(timer);
            }
            else
            {
                // Missing key; fallback
                ShowBunkerUI();
            }
        }
        else
        {
            // Bunker
            ShowBunkerUI();
        }
    }

    // ---------- VISUAL HELPERS ----------

    private void UpdateTimerDisplay(float seconds)
    {
        if (!explorationTimerText) return;

        TimeSpan ts = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        if (isCountingUp)
            explorationTimerText.text = $"Time Outside: {ts:hh\\:mm\\:ss}";
        else if (isCountingDown)
            explorationTimerText.text = $"Returning In: {ts:hh\\:mm\\:ss}";
        else
            explorationTimerText.text = "Time Outside: 00:00:00";
    }

    private void SetBackgroundScrolling(bool forward)
    {
        // Best-effort: if scrollers expose "scrollSpeed" we flip signs for return
        if (backgroundScrollers != null && bgOriginalSpeeds != null)
        {
            for (int i = 0; i < backgroundScrollers.Length; i++)
            {
                var f = backgroundScrollers[i].GetType().GetField("scrollSpeed");
                if (f != null)
                {
                    float baseSpeed = (i < bgOriginalSpeeds.Length) ? bgOriginalSpeeds[i] : (float)f.GetValue(backgroundScrollers[i]);
                    f.SetValue(backgroundScrollers[i], forward ? Mathf.Abs(baseSpeed) : -Mathf.Abs(baseSpeed));
                }
            }
        }
        if (foregroundScrollers != null && fgOriginalSpeeds != null)
        {
            for (int i = 0; i < foregroundScrollers.Length; i++)
            {
                var f = foregroundScrollers[i].GetType().GetField("scrollSpeed");
                if (f != null)
                {
                    float baseSpeed = (i < fgOriginalSpeeds.Length) ? fgOriginalSpeeds[i] : (float)f.GetValue(foregroundScrollers[i]);
                    f.SetValue(foregroundScrollers[i], forward ? Mathf.Abs(baseSpeed) : -Mathf.Abs(baseSpeed));
                }
            }
        }
    }

    private void FlipCharacter(bool faceLeft)
    {
        if (characterSprites == null) return;
        foreach (var sr in characterSprites)
        {
            if (sr) sr.flipX = faceLeft;
        }
    }
}
