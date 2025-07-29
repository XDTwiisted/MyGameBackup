using UnityEngine;
using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;

public class ExplorationDialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
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

    private Queue<InventoryItemData> foundItemQueue = new Queue<InventoryItemData>();

    public float startDelay = 8f;
    private float delayTimer = 0f;
    private bool delayPassed = false;

    public void UpdateDialogue(float deltaTime)
    {
        if (!isExploring)
            return;

        if (!delayPassed)
        {
            delayTimer += deltaTime;
            if (delayTimer >= startDelay)
            {
                delayPassed = true;
                dialogueTimer = 0f;
                ShowRandomLine();
            }
            else
            {
                return;
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
        gameObject.SetActive(true);
    }

    public void StopExploration()
    {
        isExploring = false;
        ClearFoundItemsQueue();
        ClearDialogue();
        gameObject.SetActive(false);
    }

    public void ClearDialogue()
    {
        if (dialogueText != null)
        {
            dialogueText.text = "";
            messageHistory.Clear();
        }
    }

    public void ResetDialogue()
    {
        isExploring = true;
        delayPassed = false;
        delayTimer = 0f;
        dialogueTimer = 0f;
        ClearDialogue();
        ClearFoundItemsQueue();
        gameObject.SetActive(true);
    }

    void ShowRandomLine()
    {
        if (lines.Count == 0) return;

        int index = UnityEngine.Random.Range(0, lines.Count);
        string currentTime = DateTime.Now.ToString("h:mm tt");
        string formattedLine = $"[{currentTime}] {lines[index]}";

        AddMessageToHistory(formattedLine);
    }

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
            string colorHex = GetColorForRarity(currentItem.rarity);
            string foundMessage = $"[{currentTime}] I found <color={colorHex}><b>{currentItem.itemName}</b></color>!";

            AddMessageToHistory(foundMessage);

            yield return new WaitForSeconds(foundItemDisplayTime);
        }

        showingFoundItem = false;
        dialogueTimer = 0f;
    }

    private string GetColorForRarity(ItemRarity rarity)
    {
        Color color = RarityColors.GetColor(rarity);
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
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

    public void Refresh()
    {
        gameObject.SetActive(true);
        if (dialogueText != null)
            dialogueText.enabled = true;
    }
}
