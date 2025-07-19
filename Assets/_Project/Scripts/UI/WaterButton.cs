using UnityEngine;

public class WaterButton : MonoBehaviour
{
    public PlayerStats playerStats;           // Assign in inspector
    public float thirstRestoreAmount = 30f;  // Amount thirst restored per click

    // Called when the water button is clicked
    public void OnWaterClicked()
    {
        if (playerStats != null)
        {
            playerStats.thirst = Mathf.Clamp(playerStats.thirst + thirstRestoreAmount, 0f, playerStats.maxThirst);
            playerStats.SaveStats();  // Save immediately after restoring thirst
            Debug.Log("Water drunk! Thirst is now: " + playerStats.thirst);
            gameObject.SetActive(true); // Hide the button after use
        }
        else
        {
            Debug.LogWarning("PlayerStats reference is missing in WaterButton script.");
        }
    }
}
