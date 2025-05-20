using UnityEngine;
using Oculus.Interaction;
using UnityEngine.Events;
using System;
using System.IO;
using TMPro;

public class selectGuideController : MonoBehaviour
{
    public GameObject selectGuidePokeMenu;
    public GameObject guidePokeMenu;
    public PokeMenuTextController guideController;
    public GameObject pokeButton;
    public TMP_Text leftCol;
    public TMP_Text rightCol;


    private string indexFileName = "index.txt";

    private IndexResource indexResource;
    private int verticalDistanceButton = 5;

    void Start()
    {
        // Parse the index to dictionary<DisplayText, filename>
        this.indexResource = ParseIndexToResource(indexFileName);

        // Fill in the display-collumns with the index content
        FillColumnsFromIndexResource();
    }
    IndexResource ParseIndexToResource(String filename)
    {
        // Load the file from Resources (without extension)
        TextAsset indexTextAsset = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(filename));
        if (indexTextAsset == null)
        {
            Debug.LogError("Index file not found: " + filename);
            return null;
        }

        string[] lines = indexTextAsset.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        IndexResource indexResource = null;
        Category currentCategory = null;
        bool nextCol = false;

        foreach (string rawLine in lines)
        {
            string line = rawLine.Trim();

            if (line.StartsWith("#"))
            {
                // Title line
                string title = line.Substring(1).Trim();
                indexResource = new IndexResource(title);
            }
            else if (line == "@next_col")
            {
                nextCol = true;
            }
            else if (!line.StartsWith("-") && line.Length > 0)
            {
                // Category title
                currentCategory = new Category(line, nextCol);
                nextCol = false;
                indexResource?.addCategory(currentCategory);
            }
            else if (line.StartsWith("-") && currentCategory != null)
            {
                // Content line: - displaytext >filename
                string content = line.Substring(1).Trim();
                int sepIndex = content.IndexOf('>');
                if (sepIndex > 0)
                {
                    string displayText = content.Substring(0, sepIndex).Trim();
                    string fileName = content.Substring(sepIndex + 1).Trim();
                    currentCategory.addContent(displayText, fileName);
                }
            }
        }

        return indexResource;
    }

    void FillColumnsFromIndexResource()
    {
        if (indexResource == null) return;

        // Prepare string builders for both columns
        System.Text.StringBuilder leftBuilder = new System.Text.StringBuilder();
        System.Text.StringBuilder rightBuilder = new System.Text.StringBuilder();

        // Start positions for buttons
        Vector3 leftStart = leftCol.transform.position;
        Vector3 rightStart = rightCol.transform.position;
        int leftRow = 0;
        int rightRow = 0;

        // Track which column to use
        bool isLeft = true;

        // Print the index title at the top of both columns
        leftBuilder.AppendLine(indexResource.title);
        rightBuilder.AppendLine(indexResource.title);

        foreach (var category in indexResource.GetCategories())
        {
            // Decide which column to use
            if (category.shouldNextCollumn())
                isLeft = false;

            // Add category title
            if (isLeft)
            {
                leftBuilder.AppendLine();
                leftBuilder.AppendLine(category.title);
                leftRow += 2;
            }
            else
            {
                rightBuilder.AppendLine();
                rightBuilder.AppendLine(category.title);
                rightRow += 2;
            }

            // Add entries and buttons
            foreach (var entry in category.getContents())
            {
                string displayText = entry.Key;
                string filename = entry.Value;

                if (isLeft)
                {
                    leftBuilder.AppendLine(displayText);

                    // Calculate button position (adjust as needed)
                    Vector3 buttonPos = leftStart + new Vector3(200, -verticalDistanceButton * leftRow, 0);
                    CreatePokeButtonAt(buttonPos, () => ShowGuide(filename));
                    leftRow++;
                }
                else
                {
                    rightBuilder.AppendLine(displayText);

                    // Calculate button position (adjust as needed)
                    Vector3 buttonPos = rightStart + new Vector3(200, -verticalDistanceButton * rightRow, 0);
                    CreatePokeButtonAt(buttonPos, () => ShowGuide(filename));
                    rightRow++;
                }
            }
        }

        // Set the text fields
        leftCol.text = leftBuilder.ToString();
        rightCol.text = rightBuilder.ToString();
    }

    // Create a new pokeButton at the specified location with the specified callback function
    public GameObject CreatePokeButtonAt(Vector3 newPosition, UnityAction callback)
    {
        // Instantiate a copy of the pokeButton
        GameObject newButton = Instantiate(pokeButton, newPosition, pokeButton.transform.rotation);

        // Get the InteractableUnityEventWrapper component
        InteractableUnityEventWrapper wrapper = newButton.GetComponent<InteractableUnityEventWrapper>();
        if (wrapper != null)
        {
            // Remove all existing listeners
            wrapper.WhenSelect.RemoveAllListeners();
            // Add the new callback
            wrapper.WhenSelect.AddListener(callback);
        }
        else
        {
            Debug.LogWarning("InteractableUnityEventWrapper not found on new pokeButton instance.");
        }

        return newButton;
    }

    public void ShowGuide(String filename)
    {
        guidePokeMenu.SetActive(true);
        guideController.fileName = filename;
        guideController.ShowMenu();
    }


    // @Deprecated
    public void ShowHeatbedGuide()
    {
        guidePokeMenu.SetActive(true);
        guideController.fileName = "heatbed";
        guideController.ShowMenu();
    }
}