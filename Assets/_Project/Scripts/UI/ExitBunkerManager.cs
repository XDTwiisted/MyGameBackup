using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ExitBunkerManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject confirmationPanel;
    public Button exitBunkerButton;
    public Button yesButton;
    public Button noButton;
    public Button returnButton;
    public TextMeshProUGUI explorationTimerText;
    public ExplorationDialogueManager explorationDialogue;

    [Header("Character")]
    public GameObject character;

    [Header("Exploration")]
    public ExplorationManager explorationManager; // Assign in Inspector

    private float timer = 0f;
    private bool isCountingUp = false;
    private bool isCountingDown = false;
    private bool isExploring = false;

    // PlayerPrefs keys
    private const string STATE_KEY = "gameState"; // "bunker", "exploring", "returning"
    private const string EXPLORE_TIME_KEY = "explorationStartTime";
    private const string RETURN_TIME_KEY = "returnStartTime";
    private const string RETURN_DURATION_KEY = "returnDuration";

    void Start()
    {
        confirmationPanel.SetActive(false);
        returnButton.gameObject.SetActive(false);
        explorationTimerText.text = "Time Outside: 00:00";

        RestoreState();

        exitBunkerButton.onClick.AddListener(() =>
        {
            confirmationPanel.SetActive(true);
        });

        yesButton.onClick.AddListener(() =>
        {
            confirmationPanel.SetActive(false);
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

            explorationDialogue.StartExploration();

            // START real-time loot generation during exploration
            if (explorationManager != null)
            {
                explorationManager.StartExploring();
            }
            else
            {
                Debug.LogWarning("ExplorationManager not assigned in ExitBunkerManager.");
            }
        });

        noButton.onClick.AddListener(() =>
        {
            confirmationPanel.SetActive(false);
        });

        returnButton.onClick.AddListener(() =>
        {
            isCountingUp = false;
            isCountingDown = true;
            isExploring = false;
            returnButton.gameObject.SetActive(false);
            explorationDialogue.StopExploration();

            // STOP real-time loot generation when returning
            if (explorationManager != null)
            {
                explorationManager.StopExploring();
            }

            DateTime now = DateTime.UtcNow;
            PlayerPrefs.SetString(RETURN_TIME_KEY, now.ToBinary().ToString());
            PlayerPrefs.SetFloat(RETURN_DURATION_KEY, timer);
            PlayerPrefs.SetString(STATE_KEY, "returning");
            PlayerPrefs.Save();
        });
    }

    void Update()
    {
        if (isCountingUp)
        {
            timer += Time.deltaTime;
            UpdateTimerDisplay(timer);
        }
        else if (isCountingDown)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                timer = 0f;
                isCountingDown = false;

                character.SetActive(true);
                exitBunkerButton.gameObject.SetActive(true);
                PlayerPrefs.SetString(STATE_KEY, "bunker");
                PlayerPrefs.Save();
            }

            UpdateTimerDisplay(timer);
        }
    }

    void UpdateTimerDisplay(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);
        int hours = totalSeconds / 3600;
        int minutes = (totalSeconds % 3600) / 60;
        int seconds = totalSeconds % 60;

        explorationTimerText.text = $"Time Outside: {hours:00}:{minutes:00}:{seconds:00}";
    }

    public void SpeedUpReturnByOneHour()
    {
        timer = Mathf.Max(0f, timer - 3600f); // subtract 3600 seconds (1 hour), but not below zero
        UpdateTimerDisplay(timer);
    }

    void RestoreState()
    {
        string state = PlayerPrefs.GetString(STATE_KEY, "bunker");

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

                // Resume real-time loot generation on restore
                if (explorationManager != null)
                {
                    explorationManager.StartExploring();
                }
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
                    isExploring = false;

                    character.SetActive(true);
                    exitBunkerButton.gameObject.SetActive(true);
                    returnButton.gameObject.SetActive(false);
                    PlayerPrefs.SetString(STATE_KEY, "bunker");
                    PlayerPrefs.Save();
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

                    // Stop loot generation while returning
                    if (explorationManager != null)
                    {
                        explorationManager.StopExploring();
                    }
                }
            }
        }
        else
        {
            isCountingUp = false;
            isCountingDown = false;
            isExploring = false;
            timer = 0f;

            character.SetActive(true);
            exitBunkerButton.gameObject.SetActive(true);
            returnButton.gameObject.SetActive(false);

            // Ensure loot generation is stopped when in bunker
            if (explorationManager != null)
            {
                explorationManager.StopExploring();
            }
        }

        UpdateTimerDisplay(timer);
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            PlayerPrefs.Save();
    }
}
