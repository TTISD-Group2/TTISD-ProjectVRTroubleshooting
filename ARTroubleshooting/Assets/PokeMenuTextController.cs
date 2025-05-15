using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PokeMenuTextController : MonoBehaviour
{
    public TMP_Text textDisplay;
    public string fileName = "textdata";
    
    [Tooltip("Font size for the text display")]
    public float fontSize = 12f;
    
    private List<string> textChunks = new List<string>();
    private int currentChunkIndex = 0;

    void Start()
    {
        textDisplay.fontSize = fontSize;
        
        textDisplay.overflowMode = TextOverflowModes.Page;
        
        LoadTextChunks();
        
        if (textChunks.Count > 0)
        {
            DisplayCurrentChunk();
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
    
    void DisplayCurrentChunk()
    {
        if (currentChunkIndex >= 0 && currentChunkIndex < textChunks.Count)
        {
            textDisplay.text = textChunks[currentChunkIndex];
            
            textDisplay.pageToDisplay = 1;
        }
    }
    
    //below not integerated yet, needs to be added to the UI when creating a second button
    public void ShowNextPage()
    {
        if (textDisplay.pageToDisplay < textDisplay.textInfo.pageCount)
        {
            textDisplay.pageToDisplay += 1;
        }
        else
        {
            if (currentChunkIndex < textChunks.Count - 1)
            {
                currentChunkIndex++;
                DisplayCurrentChunk();
            }
            else
            {
                textDisplay.text = "Einde van de tekst.";
            }
        }
    }
    
    public void ShowPreviousPage()
    {
        if (textDisplay.pageToDisplay > 1)
        {
            textDisplay.pageToDisplay -= 1;
        }
        else
        {
            if (currentChunkIndex > 0)
            {
                currentChunkIndex--;
                DisplayCurrentChunk();
                
                textDisplay.pageToDisplay = textDisplay.textInfo.pageCount;
            }
        }
    }
}