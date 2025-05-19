using UnityEngine;

public class selectGuideController : MonoBehaviour
{
    public GameObject secondPokeMenu; // Sleep hier het tweede menu GameObject in de Inspector
    public PokeMenuTextController secondMenuController; // Sleep hier het PokeMenuTextController component van het tweede menu

    public void ShowHeatbedGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "heatbed";
        secondMenuController.ShowMenu();
    }
}