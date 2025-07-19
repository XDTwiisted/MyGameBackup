using UnityEngine;

public class FoodButton : MonoBehaviour
{
    public PlayerStats playerStats;           // Assign in inspector
    public float hungerRestoreAmount = 25f;  // Amount hunger restored per click

    // This method will be called by the button's OnClick event
    public void OnFoodClicked()
    {
        if (playerStats != null)
        {
            playerStats.hunger = Mathf.Clamp(playerStats.hunger + hungerRestoreAmount, 0f, playerStats.maxHunger);
            playerStats.SaveStats();  // Save immediately after restoring hunger
            Debug.Log("Food eaten! Hunger is now: " + playerStats.hunger);
            gameObject.SetActive(true); // Hide the button after use
        }
        else
        {
            Debug.LogWarning("PlayerStats reference is missing in FoodButton script.");
        }
    }
}
