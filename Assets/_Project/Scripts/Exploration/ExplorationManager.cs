using UnityEngine;
using System;
using System.Collections.Generic;

public class ExplorationManager : MonoBehaviour
{
    public LootTable lootTable;                    // Assign in Inspector
    public float lootTickInterval = 10f;           // Seconds between loot rolls

    private float lootTimer = 0f;
    private bool isExploring = false;

    private const string LastExplorationStartTimeKey = "LastExplorationStartTime";

    public ExplorationDialogueManager explorationDialogueManager;

    public void StartExploring()
    {
        isExploring = true;
        lootTimer = 0f;
        Debug.Log("Exploration started.");

        HandleOfflineLoot();

        PlayerPrefs.SetString(LastExplorationStartTimeKey, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    public void StopExploring()
    {
        isExploring = false;
        Debug.Log("Exploration stopped.");

        PlayerPrefs.DeleteKey(LastExplorationStartTimeKey);
        PlayerPrefs.Save();

        explorationDialogueManager?.ClearFoundItemsQueue();
    }

    // Called externally with deltaTime * speedMultiplier
    public void AdvanceExplorationTime(float deltaTime)
    {
        if (!isExploring) return;

        lootTimer += deltaTime;

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
            if (loot == null || loot.itemData == null)
            {
                Debug.LogWarning("LootItem or itemData is null.");
                continue;
            }

            int quantityToAdd = Mathf.Max(1, loot.minQuantity);
            InventoryManager.Instance.AddItem(loot.itemData, quantityToAdd);
            Debug.Log($"Found {quantityToAdd}x {loot.itemData.itemName} while exploring!");

            if (explorationDialogueManager != null)
            {
                for (int i = 0; i < quantityToAdd; i++)
                {
                    explorationDialogueManager.FoundItem(loot.itemData);
                }
            }
        }
    }

    private void HandleOfflineLoot()
    {
        if (!PlayerPrefs.HasKey(LastExplorationStartTimeKey)) return;

        long binaryTime = Convert.ToInt64(PlayerPrefs.GetString(LastExplorationStartTimeKey));
        DateTime lastStartTime = DateTime.FromBinary(binaryTime);
        TimeSpan offlineDuration = DateTime.UtcNow - lastStartTime;

        if (offlineDuration.TotalSeconds <= 0) return;

        int offlineTicks = Mathf.FloorToInt((float)(offlineDuration.TotalSeconds / lootTickInterval));
        if (offlineTicks <= 0) return;

        Debug.Log($"Generating {offlineTicks} offline loot ticks based on {offlineDuration.TotalSeconds:F1} seconds offline.");

        for (int i = 0; i < offlineTicks; i++)
        {
            GenerateLoot();
        }
    }
}
