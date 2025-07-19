using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUI : MonoBehaviour
{
    public PlayerStats playerStats;

    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider thirstSlider;

    void Start()
    {
        // Set up all sliders
        SetupSlider(healthSlider);
        SetupSlider(hungerSlider);
        SetupSlider(thirstSlider);
    }

    void Update()
    {
        if (playerStats == null) return;

        healthSlider.value = Mathf.Clamp01(playerStats.health / playerStats.maxHealth);
        hungerSlider.value = Mathf.Clamp01(playerStats.hunger / playerStats.maxHunger);
        thirstSlider.value = Mathf.Clamp01(playerStats.thirst / playerStats.maxThirst);
    }

    private void SetupSlider(Slider slider)
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.interactable = false;
        }
    }
}
