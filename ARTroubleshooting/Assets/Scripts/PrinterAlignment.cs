using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PrinterAlignment : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject printerModel;
    [SerializeField] private GameObject pointMarkerPrefab; // Add this to visualize your touch points

    [SerializeField] private Transform rightControllerTransform;


    [Header("Calibration Settings")]
    [SerializeField] private int requiredPoints = 4;

    private List<Vector3> worldPoints = new List<Vector3>();
    private Vector3[] modelPoints;
    private List<GameObject> pointMarkers = new List<GameObject>();

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private LineRenderer lineRenderer;
    public float rayLength = 10f;
    private void Start()
    {
        Debug.Log("PrinterAlignment script initialized!");

        // Check if components are assigned
        if (raycastManager == null)
        {
            raycastManager = FindObjectOfType<ARRaycastManager>();
            if (raycastManager == null)
                Debug.LogError("ARRaycastManager not found! Please assign it in the inspector.");
        }

        if (printerModel == null)
            Debug.LogError("Printer model not assigned! Please assign it in the inspector.");

        // Define model points in local space (corners of the build plate)
        modelPoints = new Vector3[]
        {
            new Vector3(0.1f, 0.1f, 0f),
            new Vector3(0.1f, -0.1f, 0f),
            new Vector3(-0.1f, -0.1f, 0f),
            new Vector3(-0.1f, 0.1f, 0f)
        };

        // Add line renderer for visualization
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.01f;
        lineRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        lineRenderer.material.color = Color.red;
        lineRenderer.positionCount = 2;

    }

    private void Update()
    {

        

        if (worldPoints.Count >= requiredPoints)
            return;
            
            
            // Get reference to controller transforms - add this to your Start method
            Transform rightHandAnchor = GameObject.Find("RightHandAnchor")?.transform;
            Transform leftHandAnchor = GameObject.Find("LeftHandAnchor")?.transform;
            // Set laser line positions
            lineRenderer.SetPosition(0, rightHandAnchor.position);
            lineRenderer.SetPosition(1, rightHandAnchor.position + rightHandAnchor.forward * rayLength);

                // Then in your Update method
            if (rightHandAnchor != null &&
                (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) ||
                OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)))
            {
            Ray ray = new Ray(rightHandAnchor.position, rightHandAnchor.forward);
            // Perform raycast with this ray


            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, rayLength))
            {
                // Hit something (e.g. your room mesh)
                Vector3 hitPoint = hit.point;
                Debug.Log($"Hit mesh at {hitPoint}");

                worldPoints.Add(hitPoint);

                if (pointMarkerPrefab != null)
                {
                    Instantiate(pointMarkerPrefab, hitPoint, Quaternion.identity);
                }

                if (worldPoints.Count == requiredPoints)
                {
                    AlignModel();
                }
            }
            else
            {
                Debug.Log("Physics raycast did not hit any mesh");
            }
        }
           

        // Handle touch input
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            HandleTouch(touchPosition);
        }
        
        // For testing in the Unity editor
        #if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            HandleTouch(Input.mousePosition);
        }
        #endif
    }
    
    private void HandleTouch(Vector2 touchPosition)
    {
        Debug.Log($"Processing touch at {touchPosition}");
        
            
        if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            worldPoints.Add(hitPose.position);
            Debug.Log($"Captured point {worldPoints.Count}/{requiredPoints}: {hitPose.position}");

            // Visualize the touch point
            if (pointMarkerPrefab != null)
            {
                GameObject marker = Instantiate(pointMarkerPrefab, hitPose.position, Quaternion.identity);
                pointMarkers.Add(marker);
            }

            // If we've collected all required points, align the model
            if (worldPoints.Count == requiredPoints)
            {
                AlignModel();
            }
        }
        else
        {
            Debug.Log("Raycast did not hit any trackable surface");
        }
    }

    private void AlignModel()
    {
        if (modelPoints.Length != worldPoints.Count)
        {
            Debug.LogError("Mismatch between model points and world points count.");
            return;
        }

        // Convert worldPoints (List<Vector3>) to Vector3[]
        Vector3[] worldPointsArray = worldPoints.ToArray();
        
        try
        {
            // Create an instance of KabschSolver
            KabschSolver kabschSolver = new KabschSolver();

            // Properly convert and pass your points to the solver
            Vector4[] targetPoints = new Vector4[worldPoints.Count];
            
            for (int i = 0; i < modelPoints.Length; i++)
            {
                targetPoints[i] = new Vector4(worldPoints[i].x, worldPoints[i].y, worldPoints[i].z, 1.0f);
            }

            // Call the SolveKabsch method with the proper parameters
            Matrix4x4 transformation = kabschSolver.SolveKabsch(modelPoints, targetPoints, true, true);

            // Apply the transformation to the printer model
            printerModel.transform.position = transformation.GetColumn(3);
            //printerModel.transform.rotation = transformation.rotation;
            
            Debug.Log("Model aligned successfully!");
            
            // Make your model visible if it was hidden
            printerModel.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during model alignment: {e.Message}");
        }
    }
    
    // Method to reset the calibration process
    public void ResetCalibration()
    {
        worldPoints.Clear();
        
        // Remove visualization markers
        foreach (GameObject marker in pointMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        pointMarkers.Clear();
        
        Debug.Log("Calibration reset. Tap screen to capture new points.");
    }
}