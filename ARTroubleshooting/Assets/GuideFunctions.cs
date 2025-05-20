using UnityEngine;

/// <summary>
/// Class containing functions to show repair guides for various components
/// </summary>
public class GuideFunctions : MonoBehaviour
{
    // References to UI elements
    public GameObject secondPokeMenu;
    public PokeMenuTextController secondMenuController;

    #region Guide Functions

    public void ShowHeatbedGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "heatbed";
        secondMenuController.ShowMenu();
    }

    public void ShowCircuitBoardwiFiAntennacameraAcBoardReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "circuit-boardwi-fi-antennacamera-ac-board-replacement";
        secondMenuController.ShowMenu();
    }

    public void ShowCircuitBoardwiFiAntennacameraCameraReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "circuit-boardwi-fi-antennacamera-camera-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowCircuitBoardwiFiAntennacameraMainboardReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "circuit-boardwi-fi-antennacamera-mainboard-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowCircuitBoardwiFiAntennacameraReplacementOfPowerSwitchGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "circuit-boardwi-fi-antennacamera-replacement_of_power_switch";
        secondMenuController.ShowMenu();
    }

    public void ShowCircuitBoardwiFiAntennacameraThBoardReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "circuit-boardwi-fi-antennacamera-th-board-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowCircuitBoardwiFiAntennacameraUsbCCableReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "circuit-boardwi-fi-antennacamera-usb-c-cable-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowCircuitBoardwiFiAntennacameraWiFiAntennaReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "circuit-boardwi-fi-antennacamera-wi-fi-antenna-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowDisplayTouchscreenReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "display-touchscreen-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowOthersPowerSupplyReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "others-power-supply-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowPrinterBodyBottomBaseGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "printer-body-bottom-base";
        secondMenuController.ShowMenu();
    }

    public void ShowPrinterBodyPrinterGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "printer-body-printer";
        secondMenuController.ShowMenu();
    }

    public void ShowPrinterBodySpoolHolderGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "printer-body-spool-holder";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadA1ExtruderMotorReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-A1_Extruder_motor_replacement";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadA1ExtruderGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-a1-extruder";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadExtruderReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-extruder-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadFilamentCutterReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-filament-cutter-replacement";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadFilamentSensorReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-filament-sensor-replacement";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadGuideForReplacingYAxisComponentsGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-guide-for-Replacing-y-axis-components";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadHotendCoolingFanGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-hotend-cooling-fan";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadHotendHeatingAssemblyReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-hotend-heating-assembly-replacement";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadThBoardReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-th-board-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowToolheadToolheadGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "toolhead-toolhead";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltA1YBeltReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-a1-y-belt-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltA1ZAxisLeadscrewKitReplacementGuidesGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-a1-z-axis-leadscrew-kit-replacement-guides";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltA1ZBeltReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-a1-z-belt-replacement-guide";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltGuideForReplacingYAxisComponentsGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-guide-for-Replacing-y-axis-components";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltReplaceXBeltGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-replace-x-belt";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltXAxisGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-x-axis";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltYAxisGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-y-axis";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltZAxisGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-z-axis";
        secondMenuController.ShowMenu();
    }

    public void ShowXyzAxismotorbeltZMotorReplacementGuide()
    {
        secondPokeMenu.SetActive(true);
        secondMenuController.fileName = "xyz-axismotorbelt-z-motor-replacement-guide";
        secondMenuController.ShowMenu();
    }

    #endregion
}

