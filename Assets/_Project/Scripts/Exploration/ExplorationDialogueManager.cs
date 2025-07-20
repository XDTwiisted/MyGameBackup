using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

public class ExplorationDialogueManager : MonoBehaviour
{
    public TextMeshProUGUI dialogueText; // Assign this in Inspector
    public float dialogueInterval = 15f;

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
       "Found a torn map fragment.",
       "Shadows stretch long across the land.",
       "An old radio crackles faintly.",
       "Tracks lead into the forest.",
       "A discarded can rattles in the breeze.",
       "An eerie quiet fills the air.",
       "Clouds gather on the horizon.",
       "Found a cracked lantern.",
       "Broken fenceposts mark a boundary.",
       "The smell of damp earth is strong.",
       "A shattered mirror reflects nothing.",
       "Fallen leaves crunch beneath my feet.",
       "A rusty blade lies half-buried.",
       "Found a bent spoon.",
       "The sky is overcast and heavy.",
       "A distant howl sends chills.",
       "An old tire lies nearby.",
       "Found a pile of bones.",
       "A collapsed shack blocks the path.",
       "Scorch marks on nearby rocks.",
       "Found some dried herbs.",
       "An abandoned bike leans on a tree.",
       "The ground feels unstable here.",
       "A faint glow flickers in the distance.",
       "Found a torn piece of cloth.",
       "The wind whistles through empty windows.",
       "An old book lies open, pages tattered.",
       "Broken chains lie in the dirt.",
       "Found a half-burned candle.",
       "Rust flakes off a nearby pipe.",
       "A spider web glistens with dew.",
       "Faint footprints in the dust.",
       "Found a cracked pot.",
       "The air smells faintly of gasoline.",
       "A worn-out boot lies nearby.",
       "An empty bottle rolls on the ground.",
       "Found a scrap of paper with scribbles.",
       "Dark clouds block the sun.",
       "A crow circles overhead.",
       "Found a broken compass.",
       "An old calendar flutters in the wind.",
       "Rusty nails scatter on the floor.",
       "Found a frayed rope.",
       "The ground is littered with debris.",
       "A shutter creaks in the breeze.",
       "Found a broken watchband.",
       "A lone tree stands against the sky.",
       "Found a dented helmet.",
       "An old tire track fades away.",
       "Found a rusty key.",
       "A bird's nest sits empty.",
       "Faded footprints in the mud.",
       "Found a torn photograph.",
       "An abandoned cart lies overturned.",
       "Scattered papers flutter in the wind.",
       "Found a broken bottle.",
       "The air is thick with dust.",
       "An old lantern hangs from a post.",
       "Found a worn leather glove.",
       "Echoes of footsteps bounce off the walls.",
       "A faint smell of smoke lingers.",
       "Found a rusty bolt.",
       "A cracked window lets in cold air.",
       "An old tin can rattles softly.",
       "Found a bent nail.",
       "The sky darkens with gathering storm clouds.",
       "An abandoned radio plays static.",
       "Found a torn piece of canvas.",
       "The ground is soft and muddy here.",
       "An empty street stretches ahead.",
       "Found a broken pencil.",
       "Wind rustles through dead leaves.",
       "A distant bell tolls faintly.",
       "Found a cracked mug.",
       "An old boot lies half-buried.",
       "The sun peeks through thick clouds.",
       "Found a faded signpost.",
       "Shadows dance in the fading light.",
       "An empty cage hangs from a tree.",
       "Found a rusted chain link.",
       "The silence is almost deafening.",
       "An old clock ticks somewhere nearby.",
       "Found a discarded syringe.",
       "The ground trembles slightly.",
       "An empty bottle rolls past.",
       "Found a torn piece of rope.",
       "Cold wind bites at exposed skin.",
       "An abandoned vehicle sits rusting.",
       "Found a broken watch face."
    };

    private List<string> messageHistory = new List<string>();
    private const int maxMessages = 10;

    private bool isExploring = false;

    void Update()
    {
        if (isExploring)
        {
            dialogueTimer += Time.deltaTime;
            Debug.Log($"Dialogue timer: {dialogueTimer}/{dialogueInterval}");

            if (dialogueTimer >= dialogueInterval)
            {
                Debug.Log("Showing new dialogue line...");
                ShowRandomLine();
                dialogueTimer = 0f;
            }
        }
    }

    public void StartExploration()
    {
        isExploring = true;
        dialogueTimer = 0f;
        ShowRandomLine(); // Show one immediately when starting
    }

    public void StopExploration()
    {
        isExploring = false;
    }

    void ShowRandomLine()
    {
        if (lines.Count == 0) return;

        int index = UnityEngine.Random.Range(0, lines.Count);
        string currentTime = DateTime.Now.ToString("h:mm tt");
        string formattedLine = $"[{currentTime}] {lines[index]}";

        messageHistory.Add(formattedLine);

        if (messageHistory.Count > maxMessages)
            messageHistory.RemoveAt(0);

        dialogueText.text = string.Join("\n", messageHistory);
        Debug.Log("Exploration Log: " + formattedLine);
    }
}
