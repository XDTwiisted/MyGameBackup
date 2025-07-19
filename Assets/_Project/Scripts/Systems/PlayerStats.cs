using UnityEngine;
using System;

public class PlayerStats : MonoBehaviour
{
    // Max values
    public float maxHealth = 100f;
    public float maxHunger = 100f;
    public float maxThirst = 100f;

    // Current values
    public float health = 100f;
    public float hunger = 100f;
    public float thirst = 100f;

    // Decay rates (units per second)
    public float healthDecayRate = 1f;
    public float hungerDecayRate = 3f;
    public float thirstDecayRate = 5f;

    public static PlayerStats Instance;


    void Start()
    {
        // Load saved stats if they exist
        if (PlayerPrefs.HasKey("SavedHealth")) health = PlayerPrefs.GetFloat("SavedHealth");
        if (PlayerPrefs.HasKey("SavedHunger")) hunger = PlayerPrefs.GetFloat("SavedHunger");
        if (PlayerPrefs.HasKey("SavedThirst")) thirst = PlayerPrefs.GetFloat("SavedThirst");

        // Apply decay based on time away
        if (PlayerPrefs.HasKey("LastClosedTime"))
        {
            string savedTimeStr = PlayerPrefs.GetString("LastClosedTime");
            DateTime savedTime = DateTime.Parse(savedTimeStr);
            TimeSpan timeAway = DateTime.Now - savedTime;
            float secondsAway = (float)timeAway.TotalSeconds;

            // Calculate losses
            float hungerLoss = hungerDecayRate * secondsAway;
            float thirstLoss = thirstDecayRate * secondsAway;

            float estimatedHunger = hunger - hungerLoss;
            float estimatedThirst = thirst - thirstLoss;

            // Clamp hunger and thirst
            hunger = Mathf.Clamp(estimatedHunger, 0f, maxHunger);
            thirst = Mathf.Clamp(estimatedThirst, 0f, maxThirst);

            // Calculate how long hunger or thirst were at zero or below
            float hungerZeroDuration = estimatedHunger < 0 ? Mathf.Abs(estimatedHunger) / hungerDecayRate : 0f;
            float thirstZeroDuration = estimatedThirst < 0 ? Mathf.Abs(estimatedThirst) / thirstDecayRate : 0f;

            float maxZeroDuration = Mathf.Max(hungerZeroDuration, thirstZeroDuration);

            // Apply health decay for that duration
            float healthLoss = healthDecayRate * maxZeroDuration;
            health = Mathf.Clamp(health - healthLoss, 0f, maxHealth);
        }
    }

    void Update()
    {
        // Decay hunger and thirst in real-time
        hunger -= hungerDecayRate * Time.deltaTime;
        thirst -= thirstDecayRate * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0f, maxHunger);
        thirst = Mathf.Clamp(thirst, 0f, maxThirst);

        // Health decays only if hunger or thirst is zero
        if (hunger <= 0f || thirst <= 0f)
        {
            health -= healthDecayRate * Time.deltaTime;
        }

        health = Mathf.Clamp(health, 0f, maxHealth);
    }

    public void AdjustHunger(int amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0, 100);
        // update UI here
    }

    public void AdjustThirst(int amount)
    {
        thirst = Mathf.Clamp(thirst + amount, 0, 100);
        // update UI here
    }


    void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveStats();
        }
    }

    void OnApplicationQuit()
    {
        SaveStats();
    }

    // Call this whenever stats are updated externally (like eating food)
    public void SaveStats()
    {
        PlayerPrefs.SetFloat("SavedHealth", health);
        PlayerPrefs.SetFloat("SavedHunger", hunger);
        PlayerPrefs.SetFloat("SavedThirst", thirst);
        PlayerPrefs.SetString("LastClosedTime", DateTime.Now.ToString());
        PlayerPrefs.Save();
    }
    public void RestoreHunger(float amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger);
        SaveStats();
    }

    public void RestoreThirst(float amount)
    {
        thirst = Mathf.Clamp(thirst + amount, 0f, maxThirst);
        SaveStats();
    }
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
}
