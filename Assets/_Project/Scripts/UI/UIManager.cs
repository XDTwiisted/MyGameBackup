using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{


    [Header("Player Stats UI")]
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;
    public TextMeshProUGUI clockText;
    public TextMeshProUGUI explorationTimerText;


    // Optional: Update health/hunger/thirst sliders from PlayerStats
    public void UpdatePlayerStatsUI(float health, float hunger, float thirst)
    {
        if (healthSlider != null)
            healthSlider.value = health;
        if (hungerSlider != null)
            hungerSlider.value = hunger;
        if (thirstSlider != null)
            thirstSlider.value = thirst;
    }

    // Optional: Update clock UI
    public void UpdateClock(string timeString)
    {
        if (clockText != null)
            clockText.text = timeString;
    }

    // Optional: Update exploration timer UI
    public void UpdateExplorationTimer(string timerString)
    {
        if (explorationTimerText != null)
            explorationTimerText.text = timerString;
    }
}
