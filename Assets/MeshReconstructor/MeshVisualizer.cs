using Bakhanov.VoxelSet;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(PointStorage))]
public class MeshVisualizer : MonoBehaviour
{
    public bool drawMesh = true;
    public bool influenceRegion2 = true;
    public bool energyFunction2 = true;
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

        IPDMeshCreator = new IPDMeshCreator(pointStorage.voxelSet, smartUpdate, influenceRegion2, energyFunction2, regionAngle);

        ReadFle();
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

    public void ReadFle()
    {
        string filePath = "C:\\Users\\artem\\Unity Projects\\ARTest 3\\startpoints10.txt";
        if (!File.Exists(filePath)) return;

        string[] lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            float x = float.Parse(line.Split(' ')[0]);
            float y = float.Parse(line.Split(' ')[1]);
            float z = float.Parse(line.Split(' ')[2]);
            pointStorage.voxelSet.AddPoint(1, new Vector3(x, y, z), 1, Vector3.forward, false);
        }
        pointStorage.voxelSet.Update();
        //VoxelSet_NewActivePointsEvent(null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
