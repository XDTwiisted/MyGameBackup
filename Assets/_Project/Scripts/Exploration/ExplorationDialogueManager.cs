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
    private Queue<string> foundItemQueue = new Queue<string>();

    // Delay before starting dialogue
    public float startDelay = 8f;
    private float delayTimer = 0f;
    private bool delayPassed = false;

    void Update()
    {
        if (!isExploring)
            return;

        // Wait until delay passes before showing dialogue lines
        if (!delayPassed)
        {
            delayTimer += Time.deltaTime;
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

        dialogueTimer += Time.deltaTime;

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
        // Don't show dialogue immediately; wait for delay in Update()
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

    public void FoundItem(string itemName)
    {
        foundItemQueue.Enqueue(itemName);

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
            string currentItem = foundItemQueue.Dequeue();
            string currentTime = DateTime.Now.ToString("h:mm tt");
            string foundMessage = $"[{currentTime}] I found <color=#00FF00>{currentItem}</color>!";

            AddMessageToHistory(foundMessage);

            yield return new WaitForSeconds(foundItemDisplayTime);
        }

        showingFoundItem = false;
        dialogueTimer = 0f;
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
