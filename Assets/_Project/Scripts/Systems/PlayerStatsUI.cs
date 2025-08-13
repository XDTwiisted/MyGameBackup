using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    public PlayerStats playerStats;

    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;

    void Awake()
    {
        // Auto-assign PlayerStats if not set
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
    }

    void Start()
    {
        SetupSlider(healthSlider);
        SetupSlider(hungerSlider);
        SetupSlider(thirstSlider);

        ForceRefresh();
    }

    void Update()
    {
        if (playerStats == null) return;

        if (healthSlider != null)
            healthSlider.value = SafeRatio(playerStats.health, playerStats.maxHealth);

        if (hungerSlider != null)
            hungerSlider.value = SafeRatio(playerStats.hunger, playerStats.maxHunger);

        if (thirstSlider != null)
            thirstSlider.value = SafeRatio(playerStats.thirst, playerStats.maxThirst);
    }

    public void ForceRefresh()
    {
        if (playerStats == null) return;

        if (healthSlider != null)
            healthSlider.value = SafeRatio(playerStats.health, playerStats.maxHealth);

        if (hungerSlider != null)
            hungerSlider.value = SafeRatio(playerStats.hunger, playerStats.maxHunger);

        if (thirstSlider != null)
            thirstSlider.value = SafeRatio(playerStats.thirst, playerStats.maxThirst);
    }

    private void SetupSlider(Slider slider)
    {
        if (slider == null) return;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.interactable = false;
    }

    private float SafeRatio(float value, float max)
    {
        if (max <= 0f) return 0f;
        return Mathf.Clamp01(value / max);
    }
}
