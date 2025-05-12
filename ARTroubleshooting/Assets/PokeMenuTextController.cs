using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PokeMenuTextController : MonoBehaviour
{
    public TMP_Text textDisplay;
    public string fileName = "textdata";

    private List<string> textChunks = new List<string>();
    private int currentIndex = 0;

    void Start()
    {
        LoadTextChunks();

        if (textChunks.Count > 0)
        {
            textDisplay.text = textChunks[0];
        }
    }

    void LoadTextChunks()
    {
        TextAsset textFile = Resources.Load<TextAsset>(fileName);
        if (textFile != null)
        {
            textChunks = new List<string>(textFile.text.Split(new[] { "\n\n" }, System.StringSplitOptions.RemoveEmptyEntries));
        }
        else
        {
            Debug.LogError("Kon tekstbestand niet laden.");
        }
    }

    // 👇 Deze functie ga je linken in de Unity Inspector aan de Interactable Unity Event
    public void ShowNextChunk()
    {
        currentIndex++;
        if (currentIndex < textChunks.Count)
        {
            textDisplay.text = textChunks[currentIndex];
        }
        else
        {
            textDisplay.text = "Einde van de tekst.";
        }
    }
}
