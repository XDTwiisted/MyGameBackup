using UnityEngine;
using System;
using System.Collections.Generic;

public class ExplorationManager : MonoBehaviour
{
    public LootTable lootTable;               // Assign your LootTable asset in Inspector
    public float lootTickInterval = 10f;     // How often (in seconds) to roll for loot while exploring

    private float lootTimer = 0f;
    private bool isExploring = false;

    private const string LastExplorationStartTimeKey = "LastExplorationStartTime";

    // Call this method to start exploration and loot generation
    public void StartExploring()
    {
        isExploring = true;
        lootTimer = 0f;
        Debug.Log("Exploration started.");

        // Handle offline loot on exploration start
        HandleOfflineLoot();

        // Save the start time for offline calculations
        PlayerPrefs.SetString(LastExplorationStartTimeKey, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    // Call this method to stop exploration and loot generation
    public void StopExploring()
    {
        isExploring = false;
        Debug.Log("Exploration stopped.");

        // Remove the saved start time when exploration stops
        PlayerPrefs.DeleteKey(LastExplorationStartTimeKey);
        PlayerPrefs.Save();
    }

    private void Update()
    {
        if (!isExploring)
            return;

        lootTimer += Time.deltaTime;

        if (lootTimer >= lootTickInterval)
        {
            lootTimer = 0f;
            GenerateLoot();
        }
    }

    private void GenerateLoot()
    {
        if (lootTable == null)
        {
            Debug.LogError("LootTable is not assigned in ExplorationManager!");
            return;
        }

        List<LootItem> lootFound = lootTable.GetLoot();

        if (lootFound == null || lootFound.Count == 0)
        {
            Debug.Log("No loot dropped this roll.");
            return;
        }

        Debug.Log($"Loot found count: {lootFound.Count}");

        foreach (var loot in lootFound)
        {
            if (loot == null)
            {
                Debug.LogWarning("LootItem is null in lootFound list.");
                continue;
            }
            if (loot.itemData == null)
            {
                Debug.LogWarning("LootItem.itemData is null.");
                continue;
            }

            int quantityToAdd = loot.minQuantity > 0 ? loot.minQuantity : 1;

            InventoryManager.Instance.AddItem(loot.itemData, quantityToAdd);
            Debug.Log($"Found {quantityToAdd}x {loot.itemData.itemName} while exploring!");
        }
    }

    private void HandleOfflineLoot()
    {
        if (!PlayerPrefs.HasKey(LastExplorationStartTimeKey))
            return;

        long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(LastExplorationStartTimeKey));
        DateTime lastStartTime = DateTime.FromBinary(binaryTime);
        TimeSpan offlineDuration = DateTime.UtcNow - lastStartTime;

        if (offlineDuration.TotalSeconds <= 0)
            return;

        int offlineTicks = Mathf.FloorToInt((float)(offlineDuration.TotalSeconds / lootTickInterval));
        if (offlineTicks <= 0)
            return;

        Debug.Log($"Generating {offlineTicks} offline loot ticks based on offline duration {offlineDuration.TotalSeconds} seconds.");

        for (int i = 0; i < offlineTicks; i++)
        {
            GenerateLoot();
        }
    }
}
