using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PointStorage))]
public class VoxelsVisualizer : MonoBehaviour
{
    private PointStorage pointStorage;
    public GameObject cube;
    public Material[] voxelMaterials;
    public bool drawVoxels = false;

    // Start is called before the first frame update
    void Start()
    {
        pointStorage = GetComponent<PointStorage>();
        if (drawVoxels)
            pointStorage.voxelSet.UpdateEvent += DrawVoxels;
    }

    private void DeleteOldVoxels()
    {
        foreach (var fooObj in GameObject.FindGameObjectsWithTag("Voxel"))
        {
            Destroy(fooObj);
        }
    }

    public void DrawVoxels()
    {
        DeleteOldVoxels();
        var voxels = pointStorage.voxelSet.Voxels;
        float voxelSize = VoxelSet.MaxColliderRadius;
        print("Voxels Start!");
        foreach (var voxel in voxels)
        {
            //if (voxel.Value.Count == 0)
            //    continue;
            var obj = Instantiate(cube);
            obj.tag = "Voxel";
            obj.transform.position = ((Vector3)voxel.Key) * voxelSize * 2 + new Vector3(voxelSize, voxelSize, voxelSize);
            obj.transform.localScale = new Vector3(1, 1, 1) * 2 * voxelSize;

            if (voxel.Value.Count == 0)
            {
                obj.GetComponent<MeshRenderer>().material = voxelMaterials[0];
            }
            else
            {
                obj.GetComponent<MeshRenderer>().material = voxelMaterials[1];
            }
        }

        print("Voxels End!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
