using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PointStorage))]
public class MeshVisualizer : MonoBehaviour
{
    public bool drawMesh = true;
    public bool influenceRegion2 = true;
    public float regionAngle = 45f;
    public bool smartUpdate = true;
    public Material meshMaterial;
    private PointStorage pointStorage;
    private IPDMeshCreator IPDMeshCreator;
    private GameObject meshObject;
    // Start is called before the first frame update
    void Start()
    {
        pointStorage = GetComponent<PointStorage>();
        pointStorage.voxelSet.NewActivePointsEvent += VoxelSet_NewActivePointsEvent;

        smartUpdate = pointStorage.smartUpdate;

        IPDMeshCreator = new IPDMeshCreator(pointStorage.voxelSet, smartUpdate, influenceRegion2, regionAngle);
    }

    private void VoxelSet_NewActivePointsEvent(NewPointsArgs e)
    {
        DrawMesh(pointStorage.voxelSet.Vertices(), IPDMeshCreator.ComputeMeshTriangles());
    }

    private void DrawMesh(Vector3[] vertices, int[] triangles)
    {
        if (!drawMesh) return;

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (this.meshObject != null)
            Destroy(this.meshObject);

        meshObject = new GameObject("Mesh", typeof(MeshFilter), typeof(MeshRenderer));
        meshObject.transform.localScale = new Vector3(1, 1, 1);
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = meshMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
