using Bakhanov.VoxelSet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PointCloudVisual : MonoBehaviour
{
    public ARPointCloudManager aRPointCloudManager;
    public Slider distanceSlider;
    public Slider voxelSizeSlider;
    public Text debugText;
    public Material[] meshMaterials;
    public Material[] voxelMaterials;
    public GameObject Ball;
    public bool useRealColors = false;
    public bool online2;
    public bool pointsInsideSphere;
    public float sphereMaxRadius = 400f;
    public GameObject cube;

    private ParticleSystem cloudSystem;
    private GameObject meshObject;
    private VoxelSet1 VoxelSet;

    private Dictionary<ulong, float> confValues = new Dictionary<ulong, float>();
    private Dictionary<ulong, int> indexes = new Dictionary<ulong, int>();
    private List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();


    private Color color;
    private float size;
    private ulong counter = 0;
    private int currentMaterial = 0;
    private float maxDistance = 1f;

    private bool particlesShown = true;
    private bool online = false;
    private bool drawVoxels = true;

    void Start()
    {
        aRPointCloudManager = FindObjectOfType<ARPointCloudManager>();

        cloudSystem = GetComponent<ParticleSystem>();
        aRPointCloudManager.pointCloudsChanged += CloudUpdate;
        color = cloudSystem.main.startColor.color;
        size = cloudSystem.main.startSize.constant;
        VoxelSet = new VoxelSet1(0.02f) { CheckRadix = true, Online2 = online2 };

        ReadFle();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.right * size, Vector3.zero);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.up * size, Vector3.zero);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.forward * size, Vector3.zero);
        Gizmos.color = Color.white;
    }

    private void CloudUpdate(ARPointCloudChangedEventArgs obj)
    {
        if (!(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Stationary))
            return;

        // here we use only the first element in array updated
        if (obj.updated.Count == 0)
            return;

        var updated = obj.updated[0];
        for (int i = 0; i < updated.positions.Length; ++i)
        {

            //if (Vector3.Distance(updated.positions[i], Camera.current.transform.position) < maxDistance)
            if (CheckPoint(updated.positions[i]))
                VoxelSet.AddPoint(updated.identifiers[i], updated.positions[i], updated.confidenceValues[i], Camera.current.transform.forward, true);
        }

        if (particlesShown)
            cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());
        if (online)
            Mesh();


        Consts.particles = VoxelSet.GetParticles();
        UpdateDebugInfo();
        if (drawVoxels)
            DrawVoxels();
    }

    private bool CheckPoint(Vector3 position)
    {
        if (Vector3.Distance(position, Camera.current.transform.position) > maxDistance)
            return false;
        if (pointsInsideSphere)
        {
            Vector2 pointOnScreen = Camera.current.WorldToScreenPoint(position);
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            Debug.Log($"{Screen.width}, {Screen.height}, {pointOnScreen}, {Vector2.Distance(pointOnScreen, screenCenter)}");
            
            if (Vector2.Distance(pointOnScreen, screenCenter) > sphereMaxRadius)
                return false;
        }

        return true;
    }

    public void Clear()
    {
        if (meshObject != null)
            Destroy(meshObject);
        VoxelSet.Clear();
        cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());
        UpdateDebugInfo();
    }

    public void Save()
    {
        string filePath = Application.persistentDataPath + "/points.txt";
        if (!File.Exists(filePath))
            File.Create(filePath);
        var points = VoxelSet.GetPoints();
        string[] lines = new string[points.Count];
        for (int i = 0; i < points.Count; ++i)
        {
            lines[i] = $"{points[i].Position.x * 100} {points[i].Position.y * 100} {points[i].Position.z * 100} cl {points[i].ConfidenceValue}";
        }
        
        File.WriteAllLines(filePath, lines);
    }

    public void ReadFle()
    {
        string filePath = "D:\\Unity Projects\\ARTest 3\\startpoints9.txt";
        if (!File.Exists(filePath)) return;

        string[] lines = File.ReadAllLines(filePath);

        foreach (var line in lines)
        {
            float x = float.Parse(line.Split(' ')[0]);
            float y = float.Parse(line.Split(' ')[1]);
            float z = float.Parse(line.Split(' ')[2]);
            VoxelSet.AddPoint(new Point(1, new Vector3(x, y, z), 1), false);
        }
        cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());
        Mesh();
    }

    public void Mesh()
    {
        ComputeMesh();
    }

    public void ThrowBall()
    {
        GameObject obj = Instantiate(Ball, Camera.current.transform.position, Camera.current.transform.rotation);
        Rigidbody rigidbody = obj.GetComponent<Rigidbody>();
        rigidbody.AddForce(Camera.current.transform.forward * 200, ForceMode.Force);
    }

    public void ChangeMeshMaterial()
    {
        currentMaterial = (currentMaterial + 1) % meshMaterials.Length;
        if (meshObject != null) {
            meshObject.GetComponent<MeshRenderer>().material = meshMaterials[currentMaterial];
        }
    }

    public void DistanceChange()
    {
        maxDistance = distanceSlider.value;

        UpdateDebugInfo();
    }

    public void VoxelSizeChange()
    {
        VoxelSet.ChangeSize(voxelSizeSlider.value);
        cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());
    }

    public void OnlineChange()
    {
        online = !online;
    }

    public void DrawVoxelsChange()
    {
        drawVoxels = !drawVoxels;
        if (drawVoxels)
            DrawVoxels();
        else 
            DeleteOldVoxels();
        
    }

    public void BuildConvexHullMesh()
    {
        Vector3[] vertices = new Vector3[particles.Count];
        for (int i = 0; i < particles.Count; ++i)
        {
            vertices[i] = particles[i].position;
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            triangles = global::ConvexHull.Generate(vertices)
        };

        mesh.RecalculateNormals();

        if (this.meshObject != null)
            Destroy(this.meshObject);

        this.meshObject = new GameObject("ConvexHull", typeof(MeshFilter), typeof(MeshRenderer));
        this.meshObject.transform.localScale = new Vector3(1, 1, 1);
        this.meshObject.GetComponent<MeshFilter>().mesh = mesh;
        this.meshObject.GetComponent<MeshRenderer>().material = meshMaterials[currentMaterial];
        this.meshObject.AddComponent<Rigidbody>();
    }

    public void AddRandomPoint()
    {
        System.Random random = new System.Random((int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        Vector3 position = new Vector3(random.Next(74, 165), random.Next(40, 195), random.Next(48, 70));
        VoxelSet.AddPoint(counter, position, 1, new Vector3(0, 0, 0), true);
        counter++;

        cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());
        Consts.particles = VoxelSet.GetParticles();

        UpdateDebugInfo();
        Mesh();
    }

    public void ShowParticles()
    {
        if (particlesShown)
            cloudSystem.SetParticles(new ParticleSystem.Particle[0]);
        else
            cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());

        particlesShown = !particlesShown;
    }

    public void DrawVoxels()
    {
        DeleteOldVoxels();
        var voxels = VoxelSet.GetVoxels();
        float voxelSize = VoxelSet1.MaxColliderRadius;
        print("Voxels Start!");
        foreach (var voxel in voxels)
        {
            //if (voxel.Value.Count == 0)
            //    continue;
            var obj = Instantiate(cube);
            obj.tag = "Voxel";
            obj.transform.position = (( Vector3) voxel.Key) * voxelSize * 2 + new Vector3(voxelSize, voxelSize, voxelSize);
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

    private void DeleteOldVoxels()
    {
        foreach (var fooObj in GameObject.FindGameObjectsWithTag("Voxel"))
        {
            Destroy(fooObj);
        }
    }

    private void ComputeMesh()
    {
        CrustMeshCreator meshCreator1 = new CrustMeshCreator(VoxelSet.GetBigPoints().ToArray());
       // IPDMeshCreator meshCreator = new IPDMeshCreator(VoxelSet);
        //int[] triangles = meshCreator.ComputeMeshTriangles();//meshCreator.ComputeMeshTriangles(radius: radius);        

        //StartCoroutine(DrawMesh(triangles));
        //DrawMesh1(triangles);
    }

    private void DrawMesh1(int[] triangles)
    {
        Mesh mesh = new Mesh
        {
            vertices = VoxelSet.PointCloud(),
            triangles = triangles
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (this.meshObject != null)
            Destroy(this.meshObject);

        meshObject = new GameObject("ConvexHull", typeof(MeshFilter), typeof(MeshRenderer));
        meshObject.transform.localScale = new Vector3(1, 1, 1);
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = meshMaterials[currentMaterial];
    }

    private IEnumerator DrawMesh(int[] triangles)
    {
        List<int> nTriangles = new List<int>();

        for (int i = 0; i < triangles.Length / 3; i++)
        {
            yield return new WaitForSeconds(1f);
            nTriangles.Add(triangles[i * 3]);
            nTriangles.Add(triangles[i * 3 + 1]);
            nTriangles.Add(triangles[i * 3 + 2]);

            Mesh mesh = new Mesh
            {
                vertices = VoxelSet.PointCloud(),
                triangles = nTriangles.ToArray()
            };

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            if (this.meshObject != null)
                Destroy(this.meshObject);

            meshObject = new GameObject("ConvexHull", typeof(MeshFilter), typeof(MeshRenderer));
            meshObject.transform.localScale = new Vector3(1, 1, 1);
            meshObject.GetComponent<MeshFilter>().mesh = mesh;
            meshObject.GetComponent<MeshRenderer>().material = meshMaterials[currentMaterial];
        }
        //meshObject.AddComponent<MeshCollider>();
        //meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        //meshObject.GetComponent<MeshCollider>().convex = false;
    }   

    private void UpdateDebugInfo()
    {
        debugText.text = $"Radius: {distanceSlider.value}\nVoxel Size: {voxelSizeSlider.value}\nParticles: {VoxelSet.GetParticles().Count}\nHidden Pa-s: {VoxelSet.GetPoints().Count}";
    }
}
