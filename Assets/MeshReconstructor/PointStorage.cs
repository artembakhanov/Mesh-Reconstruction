using Bakhanov.VoxelSet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PointStorage : MonoBehaviour
{
    public VoxelSet voxelSet = new VoxelSet();
    public float voxelSize = 0.05f;
    public bool addWhenTouched = true;
    public bool uniteNearbyPoints = true;
    public bool useRadix = true;
    public bool filterPointsFarFromCenter = true;
    public float maxDistanceFromCenter = 300f;
    public bool filterPointsWithSmallConfValue = true;
    public float minConfidenceValue = 0.2f;
    public bool filterPointsFarFromCamera = true;
    public float maxDistanceFromCamera = 1f;
    public bool smartUpdate = true;

    private ARPointCloudManager aRPointCloudManager;
    private Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

    // Start is called before the first frame update
    void Start()
    {
        aRPointCloudManager = FindObjectOfType<ARPointCloudManager>();
        if (aRPointCloudManager == null)
            throw new UnityException("No ARPointCloudManager in the scene");

        aRPointCloudManager.pointCloudsChanged += ARPointCloudManager_pointCloudsChanged;

        voxelSet.ChangeSize(voxelSize);
        voxelSet.CheckRadix = useRadix;
        voxelSet.SmartUpdate = smartUpdate;
    }

    private void ARPointCloudManager_pointCloudsChanged(ARPointCloudChangedEventArgs obj)
    {
        if (addWhenTouched && !(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Stationary))
            return;

        // here we use only the first element in array updated
        if (obj.updated.Count == 0)
            return;

        var updated = obj.updated[0];
        for (int i = 0; i < updated.positions.Length; ++i)
        {
            if (CheckPoint(updated.positions[i], updated.confidenceValues[i]))
                voxelSet.AddPoint(updated.identifiers[i], updated.positions[i], updated.confidenceValues[i], Camera.current.transform.forward, uniteNearbyPoints);
        }

        voxelSet.Update();
    }

    private bool CheckPoint(Vector3 position, float confidenceValue)
    {
        if (Vector3.Distance(position, Camera.current.transform.position) > maxDistanceFromCamera)
            return false;
        if (filterPointsWithSmallConfValue && confidenceValue < minConfidenceValue)
            return false;
        if (filterPointsFarFromCenter)
        {
            Vector2 pointOnScreen = Camera.current.WorldToScreenPoint(position);
            if (Vector2.Distance(pointOnScreen, screenCenter) > maxDistanceFromCenter)
                return false;
        }

        return true;
    }
}
