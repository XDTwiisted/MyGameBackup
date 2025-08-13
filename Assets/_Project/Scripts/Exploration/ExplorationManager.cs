using UnityEngine;
using System;
using System.Collections.Generic;

public class ExplorationManager : MonoBehaviour
{
    public static ExplorationManager Instance;

    [Header("Loot")]
    public LootTable lootTable;
    public float lootTickInterval = 10f;

    [Header("Dialogue")]
    public ExplorationDialogueManager explorationDialogueManager;

    private float lootTimer = 0f;
    private bool isExploring = false;

    private const string LastExplorationStartTimeKey = "LastExplorationStartTime";
    private const string WasExploringKey = "WasExploring";

    public bool IsExploring => isExploring;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (explorationDialogueManager == null)
            explorationDialogueManager = UnityEngine.Object.FindFirstObjectByType<ExplorationDialogueManager>(FindObjectsInactive.Include);

        if (lootTable == null)
            lootTable = UnityEngine.Object.FindFirstObjectByType<LootTable>(FindObjectsInactive.Include);
    }


    private void Start()
    {
        RestoreState();
    }

    private void Update()
    {
        AdvanceExplorationTime(Time.deltaTime);
    }

    public void StartExploring()
    {
        if (isExploring)
        {
            Debug.Log("[ExplorationManager] StartExploring called but already exploring.");
            return;
        }

        if (lootTable == null)
            Debug.LogWarning("[ExplorationManager] No LootTable assigned. Loot will not generate.");

        isExploring = true;
        lootTimer = 0f;
        Debug.Log("[ExplorationManager] Exploration started.");

        PlayerPrefs.SetString(LastExplorationStartTimeKey, DateTime.UtcNow.ToBinary().ToString());
        PlayerPrefs.SetInt(WasExploringKey, 1);
        PlayerPrefs.Save();

        if (explorationDialogueManager != null)
            explorationDialogueManager.StartExploration();
        else
            Debug.LogWarning("[ExplorationManager] No ExplorationDialogueManager assigned.");

        HandleOfflineLoot();
    }

    public void StopExploring()
    {
        if (!isExploring)
        {
            Debug.Log("[ExplorationManager] StopExploring called but not exploring.");
            return;
        }

        isExploring = false;
        Debug.Log("[ExplorationManager] Exploration stopped.");

        PlayerPrefs.DeleteKey(LastExplorationStartTimeKey);
        PlayerPrefs.SetInt(WasExploringKey, 0);
        PlayerPrefs.Save();

        if (explorationDialogueManager != null)
            explorationDialogueManager.StopExploration();
    }

    public void ReturnToBunker()
    {
        StopExploring();
        Debug.Log("[ExplorationManager] Returned to bunker: exploration stopped.");
    }

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
        if (lootTable == null) return;

        var drops = lootTable.GetLoot();
        if (drops == null || drops.Count == 0) return;

        foreach (var item in drops)
        {
            if (item.itemData.isDurable)
                InventoryManager.Instance?.AddItemInstance(item);
            else
                InventoryManager.Instance?.AddItem(item.itemData, item.quantity);

            explorationDialogueManager?.FoundItem(item.itemData);
        }
    }

    private void HandleOfflineLoot()
    {
        if (!PlayerPrefs.HasKey(LastExplorationStartTimeKey)) return;

        string savedTime = PlayerPrefs.GetString(LastExplorationStartTimeKey);
        if (!long.TryParse(savedTime, out long binaryTime)) return;

        DateTime lastTime = DateTime.FromBinary(binaryTime);
        TimeSpan elapsed = DateTime.UtcNow - lastTime;

        int ticks = Mathf.FloorToInt((float)elapsed.TotalSeconds / lootTickInterval);
        for (int i = 0; i < ticks; i++)
            GenerateLoot();

        lootTimer = (float)elapsed.TotalSeconds % lootTickInterval;
    }

    private void RestoreState()
    {
        if (PlayerPrefs.GetInt(WasExploringKey, 0) == 1)
        {
            isExploring = true;
            Debug.Log("[ExplorationManager] Restoring exploration from saved state.");

            HandleOfflineLoot();

            if (explorationDialogueManager != null)
                explorationDialogueManager.ResetDialogue();
        }
    }
}
