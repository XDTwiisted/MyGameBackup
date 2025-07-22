using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class ExplorationDialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText; // Assign in Inspector
    public float dialogueInterval = 15f;
    public float foundItemDisplayTime = 4f;

    private float dialogueTimer = 0f;

    [TextArea]
    public List<string> lines = new List<string>
    {
        "Found a rusty nail.",
        "There's an old, torn flag fluttering.",
        "Wind carries a faint scent of smoke.",
        "Looks like someone's been here recently.",
        "Steps in the dirt lead nowhere.",
        "Broken glass crunches underfoot.",
        "Found some wild berries.",
        "An abandoned campsite lies ahead.",
        "Sounds of distant thunder.",
        "Rusted machinery barely moves.",
        "Traces of footprints in the mud.",
        "A crow caws loudly nearby.",
        "Faded graffiti on the wall.",
        "Nothing but silence in this direction.",
        "A broken watch ticks faintly.",
        "Found a torn map fragment."
    };

    private List<string> messageHistory = new List<string>();
    private const int maxMessages = 10;

    private bool isExploring = false;
    private bool showingFoundItem = false;

    // Now store full InventoryItemData instead of just names
    private Queue<InventoryItemData> foundItemQueue = new Queue<InventoryItemData>();

    // Delay before starting dialogue
    public float startDelay = 8f;
    private float delayTimer = 0f;
    private bool delayPassed = false;

    // Removed original Update()

    // New method: call this every frame with scaled deltaTime
    public void UpdateDialogue(float deltaTime)
    {
        if (!isExploring)
            return;

        // Wait until delay passes before showing dialogue lines
        if (!delayPassed)
        {
            delayTimer += deltaTime;
            if (delayTimer >= startDelay)
            {
                delayPassed = true;
                dialogueTimer = 0f;  // Reset dialogue timer
                ShowRandomLine();    // Show first line after delay
            }
            else
            {
                return; // Wait for delay to finish
            }
        }

        if (showingFoundItem)
            return;

        dialogueTimer += deltaTime;

        if (dialogueTimer >= dialogueInterval)
        {
            ShowRandomLine();
            dialogueTimer = 0f;
        }
    }

    public void StartExploration()
    {
        isExploring = true;
        delayPassed = false;
        delayTimer = 0f;
        dialogueTimer = 0f;
        // Don't show dialogue immediately; wait for delay in UpdateDialogue()
    }

    public void StopExploration()
    {
        isExploring = false;
        ClearFoundItemsQueue();
        // Keep dialogue visible on returning, so no clearing here
    }

    public void ClearDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            messageHistory.Clear();
        }
    }

    void ShowRandomLine()
    {
        if (lines.Count == 0) return;

        int index = UnityEngine.Random.Range(0, lines.Count);
        string currentTime = DateTime.Now.ToString("h:mm tt");
        string formattedLine = $"[{currentTime}] {lines[index]}";

        AddMessageToHistory(formattedLine);
    }

    // Updated to accept InventoryItemData and show colored + bold text based on rarity
    public void FoundItem(InventoryItemData itemData)
    {
        if (itemData == null) return;

        foundItemQueue.Enqueue(itemData);

        if (!showingFoundItem)
        {
            StartCoroutine(ShowFoundItemMessages());
        }
    }

    IEnumerator ShowFoundItemMessages()
    {
        showingFoundItem = true;

        while (foundItemQueue.Count > 0)
        {
            InventoryItemData currentItem = foundItemQueue.Dequeue();
            string currentTime = DateTime.Now.ToString("h:mm tt");

            // Get color hex based on rarity
            string colorHex = GetColorForRarity(currentItem.rarity);

            // Wrap itemName in bold and color tags
            string foundMessage = $"[{currentTime}] I found <color={colorHex}><b>{currentItem.itemName}</b></color>!";

            AddMessageToHistory(foundMessage);

            yield return new WaitForSeconds(foundItemDisplayTime);
        }

        showingFoundItem = false;
        dialogueTimer = 0f;
    }

    // Maps rarity enum to hex color codes
    private string GetColorForRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                return "#FFFFFF"; // White
            case ItemRarity.Uncommon:
                return "#00FF00"; // Green
            case ItemRarity.Rare:
                return "#800080"; // Purple
            case ItemRarity.Legendary:
                return "#FFA500"; // Orange
            default:
                return "#FFFFFF"; // Default white
        }
    }

    void AddMessageToHistory(string message)
    {
        messageHistory.Add(message);

        if (messageHistory.Count > maxMessages)
            messageHistory.RemoveAt(0);

        dialogueText.text = string.Join("\n", messageHistory);
        Debug.Log("Exploration Log: " + message);
    }

    public void ClearFoundItemsQueue()
    {
        foundItemQueue.Clear();
        showingFoundItem = false;
        dialogueTimer = 0f;
    }
}
