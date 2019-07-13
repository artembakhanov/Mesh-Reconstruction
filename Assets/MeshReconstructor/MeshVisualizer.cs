using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PointStorage))]
public class MeshVisualizer : MonoBehaviour
{
    public bool smartUpdate = true;
    private PointStorage pointStorage;
    private IPDMeshCreator IPDMeshCreator;
    // Start is called before the first frame update
    void Start()
    {
        pointStorage = GetComponent<PointStorage>();
        pointStorage.voxelSet.NewActivePointsEvent += VoxelSet_NewActivePointsEvent;

        smartUpdate = pointStorage.smartUpdate;
    }

    private void VoxelSet_NewActivePointsEvent(NewPointsArgs e)
    {
        throw new System.NotImplementedException();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
