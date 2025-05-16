using System.Collections.Generic;
using UnityEngine;

public class PrinterPartIndicator : MonoBehaviour
{
    public Material HighlightMaterial;
    public Material NonHighLight;

    public List<GameObject> PrinterFrame;
    public List<GameObject> Screen;
    public List<GameObject> HeatBed;
    public List<GameObject> Xaxis;
    public List<GameObject> Toolhead;
    public List<GameObject> ElectricalCord;
    public List<GameObject> FillamentCutter;
    public List<GameObject> PurgeWiper;
    public List<GameObject> LiveVieuwCamera;
    public List<GameObject> Nozzle;
    public List<GameObject> SiliconeSockforHotend;
    public List<GameObject> PartCoolingFan;
    public List<GameObject> USBCcable;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hightLightToolhead();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void makeAllHighLight(List<GameObject> objects){

        foreach (GameObject objecty in objects){
            //objecty.material = HighlightMaterial;
             Renderer renderer = objecty.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = HighlightMaterial;
            }
        }

    }

    private void makeAllNotHighLight(List<GameObject> objects){

        foreach (GameObject objecty in objects){
             Renderer renderer = objecty.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = NonHighLight;
            }
        }

    }

    public void hightLightFrame(){
        makeAllHighLight(PrinterFrame);
    }

    public void unHightLightFrame(){
        makeAllNotHighLight(PrinterFrame);
    }

    public void hightLightScreen(){
        makeAllHighLight(Screen);
    }

    public void unHightLightScreen(){
        makeAllNotHighLight(Screen);
    }


    public void hightLightHeatBed(){
        makeAllHighLight(HeatBed);
    }

    public void unHightHeatBed(){
        makeAllNotHighLight(HeatBed);
    }

    public void hightLightXaxis(){
        makeAllHighLight(Xaxis);
    }

    public void unHightXaxis(){
        makeAllNotHighLight(Xaxis);
    }

    public void hightLightToolhead(){
        makeAllHighLight(Toolhead);
    }

    public void unHightToolhead(){
        makeAllNotHighLight(Toolhead);
    }

    public void hightLightElectricalCord(){
        makeAllHighLight(ElectricalCord);
    }

    public void unHightElectricalCord(){
        makeAllNotHighLight(ElectricalCord);
    }

    public void hightLightFillamentCutter(){
        makeAllHighLight(FillamentCutter);
    }

    public void unHightFillamentCutter(){
        makeAllNotHighLight(FillamentCutter);
    }


    public void hightLightPurgeWiper(){
        makeAllHighLight(PurgeWiper);
    }

    public void unHightPurgeWiper(){
        makeAllNotHighLight(PurgeWiper);
    }


    public void hightLightLiveVieuwCamera(){
        makeAllHighLight(LiveVieuwCamera);
    }

    public void unHightLiveVieuwCamera(){
        makeAllNotHighLight(LiveVieuwCamera);
    }


    public void hightLightNozzle(){
        makeAllHighLight(Nozzle);
    }

    public void unHightNozzle(){
        makeAllNotHighLight(Nozzle);
    }

    public void hightSiliconeSockforHotend(){
        makeAllHighLight(SiliconeSockforHotend);
    }

    public void unHightSiliconeSockforHotend(){
        makeAllNotHighLight(SiliconeSockforHotend);
    }

    public void hightSiliconePartCoolingFan(){
        makeAllHighLight(PartCoolingFan);
    }

    public void unHightPartCoolingFan(){
        makeAllNotHighLight(PartCoolingFan);
    }

    public void hightUSBCcable(){
        makeAllHighLight(USBCcable);
    }

    public void unHightUSBCcable(){
        makeAllNotHighLight(USBCcable);
    }


}
