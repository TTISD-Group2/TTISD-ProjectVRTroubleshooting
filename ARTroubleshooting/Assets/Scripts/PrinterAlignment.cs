using UnityEngine;

using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

using Kabsch;


public class PrinterAlignment : MonoBehaviour
{
    [Header("AR Components")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private GameObject printerModel;

    [Header("Calibration Settings")]
    [SerializeField] private int requiredPoints = 4;

    private List<Vector3> worldPoints = new List<Vector3>();
    private Vector3[] modelPoints;

    private void Start()
    {
        // Define model points in local space (e.g., corners of the build plate)
        modelPoints = new Vector3[]
        {
            new Vector3(0.1f, 0.1f, 0f),
            new Vector3(0.1f, -0.1f, 0f),
            new Vector3(-0.1f, -0.1f, 0f),
            new Vector3(-0.1f, 0.1f, 0f)
        };
    }

    private void Update()
    {
        if (worldPoints.Count >= requiredPoints)
            return;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPosition = Input.GetTouch(0).position;
            List<ARRaycastHit> hits = new List<ARRaycastHit>();

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.FeaturePoint | TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                worldPoints.Add(hitPose.position);
                Debug.Log($"Captured point {worldPoints.Count}: {hitPose.position}");

                if (worldPoints.Count == requiredPoints)
                {
                    AlignModel();
                }
            }
        }
    }

    private void AlignModel()
{
    if (modelPoints.Length != worldPoints.Count)
    {
        Debug.LogError("Mismatch between model points and world points.");
        return;
    }

    // Convert lists to arrays
    Vector3[] sourcePoints = modelPoints;
    Vector3[] targetPoints = worldPoints.ToArray();

    // Compute the transformation matrix using Kabsch algorithm
    Matrix4x4 transformation = KabschSolver.SolveKabsch(sourcePoints, targetPoints);

    // Apply the transformation to the printer model
    printerModel.transform.position = transformation.GetColumn(3);
    printerModel.transform.rotation = transformation.rotation;
}

}
