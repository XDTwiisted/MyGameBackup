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

    private const string SAVED_HEALTH_KEY = "SavedHealth";
    private const string SAVED_HUNGER_KEY = "SavedHunger";
    private const string SAVED_THIRST_KEY = "SavedThirst";
    private const string LAST_CLOSED_UTC_KEY = "LastClosedUtcBinary";
    private const string LEGACY_LAST_CLOSED_KEY = "LastClosedTime";

    private static class TimeUtil
    {
        public static DateTime UtcNow() { return DateTime.UtcNow; }

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

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        // Load saved stats if present
        if (PlayerPrefs.HasKey(SAVED_HEALTH_KEY)) health = PlayerPrefs.GetFloat(SAVED_HEALTH_KEY);
        if (PlayerPrefs.HasKey(SAVED_HUNGER_KEY)) hunger = PlayerPrefs.GetFloat(SAVED_HUNGER_KEY);
        if (PlayerPrefs.HasKey(SAVED_THIRST_KEY)) thirst = PlayerPrefs.GetFloat(SAVED_THIRST_KEY);

        // Apply offline decay using UTC
        DateTime lastClosedUtc;
        if (!TimeUtil.TryLoadUtcBinary(LAST_CLOSED_UTC_KEY, out lastClosedUtc))
        {
            // Migrate from legacy local time string if it exists
            if (PlayerPrefs.HasKey(LEGACY_LAST_CLOSED_KEY))
            {
                var legacy = PlayerPrefs.GetString(LEGACY_LAST_CLOSED_KEY, string.Empty);
                if (!string.IsNullOrEmpty(legacy))
                {
                    try
                    {
                        var parsedLocal = DateTime.Parse(legacy);
                        lastClosedUtc = parsedLocal.ToUniversalTime();
                        TimeUtil.SaveUtcBinary(LAST_CLOSED_UTC_KEY, lastClosedUtc);
                        PlayerPrefs.Save();
                    }
                    catch { }
                }
            }
        }

        if (lastClosedUtc != default)
        {
            float secondsAway = (float)(TimeUtil.UtcNow() - lastClosedUtc).TotalSeconds;
            if (secondsAway > 0f)
            {
                float hungerLoss = hungerDecayRate * secondsAway;
                float thirstLoss = thirstDecayRate * secondsAway;

                float estHunger = hunger - hungerLoss;
                float estThirst = thirst - thirstLoss;

                hunger = Mathf.Clamp(estHunger, 0f, maxHunger);
                thirst = Mathf.Clamp(estThirst, 0f, maxThirst);

                float hungerZeroDur = estHunger < 0f ? Mathf.Abs(estHunger) / Mathf.Max(0.0001f, hungerDecayRate) : 0f;
                float thirstZeroDur = estThirst < 0f ? Mathf.Abs(estThirst) / Mathf.Max(0.0001f, thirstDecayRate) : 0f;
                float zeroDur = Mathf.Max(hungerZeroDur, thirstZeroDur);

                if (zeroDur > 0f)
                {
                    float healthLoss = healthDecayRate * zeroDur;
                    health = Mathf.Clamp(health - healthLoss, 0f, maxHealth);
                }
            }
        }
    }

    void Update()
    {
        hunger -= hungerDecayRate * Time.deltaTime;
        thirst -= thirstDecayRate * Time.deltaTime;

        hunger = Mathf.Clamp(hunger, 0f, maxHunger);
        thirst = Mathf.Clamp(thirst, 0f, maxThirst);

        if (hunger <= 0f || thirst <= 0f)
        {
            health -= healthDecayRate * Time.deltaTime;
        }

        health = Mathf.Clamp(health, 0f, maxHealth);
    }

    public void AdjustHunger(int amount)
    {
        hunger = Mathf.Clamp(hunger + amount, 0f, maxHunger);
    }

    public void AdjustThirst(int amount)
    {
        thirst = Mathf.Clamp(thirst + amount, 0f, maxThirst);
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

    void OnApplicationPause(bool paused)
    {
        if (paused) SaveStats();
    }

    void OnApplicationQuit()
    {
        SaveStats();
    }

    public void SaveStats()
    {
        PlayerPrefs.SetFloat(SAVED_HEALTH_KEY, health);
        PlayerPrefs.SetFloat(SAVED_HUNGER_KEY, hunger);
        PlayerPrefs.SetFloat(SAVED_THIRST_KEY, thirst);

        TimeUtil.SaveUtcBinary(LAST_CLOSED_UTC_KEY, TimeUtil.UtcNow());

        PlayerPrefs.SetString(LEGACY_LAST_CLOSED_KEY, DateTime.Now.ToString());

        PlayerPrefs.Save();
    }
}
