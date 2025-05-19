using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using Oculus.Platform;


public class PokeMenuTextController : MonoBehaviour
{
    public TMP_Text textDisplay;
    public TMP_Text titleDisplay;
    public string fileName = "textdata";
    public float fontSize = 12f;

    private List<(string header, string content)> sections = new List<(string, string)>();
    private int currentSectionIndex = 0;

    public PrinterPartIndicator printerPartIndicator;

    private string[][] highlightable_components = new string[][]
    {
        new string[] { "Printer Frame" },
        new string[] { "Toolhead" },
        new string[] { "X-axis Assembly" },
        new string[] { "Purge Wiper" },
        new string[] { "Silicone Sock for Hotend" },
        new string[] { "Heatbed" },
        new string[] { "Bambu USB-C Cable" },
        new string[] { "Filament Cutter Lever" },
        new string[] { "Live View Camera" },
        new string[] { "Z-axis leadscrew" },
        new string[] { "Part Cooling Fan" },
        new string[] { "Nozzle" },
        new string[] { "Nozzle Wiper" },
        new string[] { "Screen" },
        new string[] { "Micro SD Card" }
    };

    /// <summary>
    /// Zoekt in de inputtekst naar alle highlightable components en geeft een lijst met unieke gevonden indices terug.
    /// </summary>
    private List<int> FindAllHighlightableComponentIndices(string inputText)
    {
        List<int> foundIndices = new List<int>();

        // Doorzoek de tekst per woord én per volledige componentnaam
        foreach (var (components, idx) in highlightable_components.Select((arr, i) => (arr, i)))
        {
            foreach (var component in components)
            {
                // Zoek op volledige naam, hoofdletterongevoelig
                if (inputText.IndexOf(component, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (!foundIndices.Contains(idx))
                        foundIndices.Add(idx);
                }
            }
        }

        return foundIndices;
    }

    private void Highlight(List<int> indices)
    {
        if (printerPartIndicator == null) return;
        foreach (int idx in indices)
        {
            switch (idx)
            {
                case 0: printerPartIndicator.hightLightFrame(); break;
                case 1: printerPartIndicator.hightLightToolhead(); break;
                case 2: printerPartIndicator.hightLightXaxis(); break;
                case 3: printerPartIndicator.hightLightPurgeWiper(); break;
                case 4: printerPartIndicator.hightSiliconeSockforHotend(); break;
                case 5: printerPartIndicator.hightLightHeatBed(); break;
                case 6: printerPartIndicator.hightUSBCcable(); break;
                case 7: printerPartIndicator.hightLightFillamentCutter(); break;
                case 8: printerPartIndicator.hightLightLiveVieuwCamera(); break;
                case 9:  break; // Z-axis leadscrew, geen aparte functie? Gebruik Xaxis als placeholder
                case 10: break;
                case 11: printerPartIndicator.hightLightNozzle(); break;
                case 12: break; // Nozzle Wiper, geen aparte functie? Gebruik PurgeWiper als placeholder
                case 13: printerPartIndicator.hightLightScreen(); break;
                case 14: break; // Micro SD Card, geen aparte functie? Gebruik USBCcable als placeholder
            }
        }
    }

    private string FormatSectionText(string input)
    {
        // Verwijder PAGEBREAK markers
        string formatted = input.Replace("===PAGE_BREAK===", "");

        // Verwijder alle ### headers (en eventuele whitespace erna)
        formatted = Regex.Replace(formatted, @"^###.*\n?", "", RegexOptions.Multiline);

        // Verwijder alle #### subheaders (en eventuele whitespace erna)
        formatted = Regex.Replace(formatted, @"^####.*\n?", "", RegexOptions.Multiline);

        // Zet **tekst** om naar <b>tekst</b>
        formatted = Regex.Replace(formatted, @"\*\*(.+?)\*\*", "<b>$1</b>");

        // Eventueel: extra whitespace aan begin/einde verwijderen
        formatted = formatted.Trim();

        return formatted;
    }

    void Start()
    {
        textDisplay.fontSize = fontSize;
        LoadSections();

        if (sections.Count > 0)
        {
            DisplayCurrentSection();
        }
    }

    void LoadSections()
    {
        TextAsset textFile = Resources.Load<TextAsset>(fileName);
        if (textFile == null)
        {
            Debug.LogError("Kon tekstbestand niet laden.");
            return;
        }

        string text = textFile.text;

        // Extract title (first line starting with "# ")
        string title = "";
        var titleMatch = Regex.Match(text, @"^# (.+)$", RegexOptions.Multiline);
        if (titleMatch.Success)
        {
            title = titleMatch.Groups[1].Value.Trim();
        }
        titleDisplay.text = title;

        // Split into sections by "### "
        var sectionMatches = Regex.Split(text, @"(?=^### )", RegexOptions.Multiline);
        foreach (var section in sectionMatches)
        {
            if (section.StartsWith("### "))
            {
                // Extract header and content
                var headerMatch = Regex.Match(section, @"^### (.+)$", RegexOptions.Multiline);
                string header = headerMatch.Success ? headerMatch.Groups[1].Value.Trim() : "";
                string content = section.Substring(headerMatch.Length).Trim();
                sections.Add((header, content));
            }
        }
    }

    void DisplayCurrentSection()
    {
        if (currentSectionIndex >= 0 && currentSectionIndex < sections.Count)
        {
            var section = sections[currentSectionIndex];
            string formattedContent = FormatSectionText(section.content);
            textDisplay.text = $"<b>{section.header}</b>\n\n{formattedContent}";
            textDisplay.pageToDisplay = 1;
            textDisplay.ForceMeshUpdate();

            printerPartIndicator.clearHighlights();

            List<int> highlightIndices = FindAllHighlightableComponentIndices(section.content);
            Highlight(highlightIndices);
        }
    }

    public void ShowNextPage()
    {
        if (currentSectionIndex < sections.Count - 1)
        {
            currentSectionIndex++;
            DisplayCurrentSection();
        }
    }

    public void ShowPreviousPage()
    {
        if (currentSectionIndex > 0)
        {
            currentSectionIndex--;
            DisplayCurrentSection();
        }
    }
}