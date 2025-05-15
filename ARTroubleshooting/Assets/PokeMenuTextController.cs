using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PokeMenuTextController : MonoBehaviour
{
    public TMP_Text textDisplay;
    public string fileName = "textdata";
    
    [Tooltip("Maximum characters per page")]
    public int maxCharsPerPage = 200; // we need to adjust this based on testing
    
    private List<string> textChunks = new List<string>();
    private List<string> currentPages = new List<string>();
    private int currentChunkIndex = 0;
    private int currentPageIndex = 0;

    void Start()
    {
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
            currentPages = SplitTextIntoPages(textChunks[currentChunkIndex]);
            currentPageIndex = 0;
            
            if (currentPages.Count > 0)
            {
                textDisplay.text = currentPages[currentPageIndex];
            }
        }
    }

    List<string> SplitTextIntoPages(string text)
    {
        List<string> pages = new List<string>();
        
        if (text.Length <= maxCharsPerPage)
        {
            pages.Add(text);
            return pages;
        }
        
        //avoid cutting off mid-word
        string[] words = text.Split(' ');
        string currentPage = "";
        
        foreach (string word in words)
        {
            // Check if adding this word would exceed page limit otherwise add it to the next page
            if ((currentPage + " " + word).Length > maxCharsPerPage && currentPage.Length > 0)
            {
                pages.Add(currentPage);
                currentPage = word;
            }
            else
            {
                if (currentPage.Length > 0)
                {
                    currentPage += " ";
                }
                currentPage += word;
            }
        }
        
        if (currentPage.Length > 0)
        {
            pages.Add(currentPage);
        }
        
        return pages;
    }

}