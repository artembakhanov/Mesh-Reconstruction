using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PointCloudVisual : MonoBehaviour
{
    public ARPointCloudManager aRPointCloudManager;
    public Slider radiusSlider;
    public Slider voxelSizeSlider;
    public Text debugText;
    public Material[] meshMaterials;
    public Material[] voxelMaterials;
    public GameObject Ball;
    public bool useRealColors = false;
    public GameObject cube;

    private ParticleSystem cloudSystem;
    private GameObject meshObject;
    private VoxelSet VoxelSet = new VoxelSet(0.02f);

    private Dictionary<ulong, float> confValues = new Dictionary<ulong, float>();
    private Dictionary<ulong, int> indexes = new Dictionary<ulong, int>();
    private List<ParticleSystem.Particle> particles = new List<ParticleSystem.Particle>();


    private Color color;
    private float size;
    private ulong counter = 0;
    private int currentMaterial = 0;
    private float radius = 20f;

    private bool particlesShown = true;
    private bool online = false;
    private bool drawVoxels = true;

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
            lines[i] = $"{points[i].Position.x * 100} {points[i].Position.y * 100} {points[i].Position.z * 100}";
        }
        
        File.WriteAllLines(filePath, lines);
    }

    public void Mesh()
    {
        StartCoroutine(ComputeMesh());
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

    public void RadiusChange()
    {
        radius = radiusSlider.value;

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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = global::ConvexHull.Generate(vertices);

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
        Vector3 position = new Vector3(random.Next(74, 165), random.Next(40, 195), 48);
        VoxelSet.AddPoint(counter, position, 1, Camera.current.transform.forward, true);
        counter++;

        cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());
        GlobalVars.particles = VoxelSet.GetParticles();

        UpdateDebugInfo();
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
        float voxelSize = VoxelSet.ColliderRadius;
        foreach (var voxel in voxels)
        {
            if (voxel.Value.Count == 0)
                continue;
            var obj = Instantiate(cube);
            obj.tag = "Voxel";
            obj.transform.position = ((Vector3) voxel.Key) * voxelSize * 2 + new Vector3(voxelSize, voxelSize, voxelSize);
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
    }

    private void DeleteOldVoxels()
    {
        foreach (var fooObj in GameObject.FindGameObjectsWithTag("Voxel"))
        {
            Destroy(fooObj);
        }
    }

    void Start()
    {
        cloudSystem = GetComponent<ParticleSystem>();
        aRPointCloudManager.pointCloudsChanged += CloudUpdate;
        color = cloudSystem.main.startColor.color;
        size = cloudSystem.main.startSize.constant;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.right * size, Vector3.zero);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(Vector3.up * size, Vector3.zero);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Vector3.forward * size, Vector3.zero);
        Gizmos.color = Color.white;
    }

    private IEnumerator RecordFrame()
    {
        yield return new WaitForEndOfFrame();
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        var particles = VoxelSet.GetParticles();
        var points = VoxelSet.GetPoints();
        string fullPath = Application.persistentDataPath + "/text.png";
        byte[] _bytes = texture.EncodeToPNG();
        //File.WriteAllBytes(fullPath, _bytes);

        for (int i = 0; i < particles.Count; ++i) {
            Vector3 screenPosition = Camera.current.WorldToScreenPoint(particles[i].position);
            
            try
            {
               
                if (!(0 <= (int)screenPosition.x && (int) screenPosition.x < Screen.width
                    &&
                    0 <= (int)screenPosition.y && (int)screenPosition.y < Screen.height))
                    continue;
                Color color = texture.GetPixel((int)screenPosition.x, (int)screenPosition.y);
                points[i].Color = color;
                var temp = particles[i];
                particles[i] = new ParticleSystem.Particle
                {
                    startColor = color,
                    startSize = temp.startSize,
                    position = temp.position,
                    remainingLifetime = temp.remainingLifetime
                };
            } catch (Exception) { }

            //cloudSystem.SetParticles(particles.ToArray());
            GlobalVars.particles = VoxelSet.GetParticles();
        }
    }

    private void UpdateColors()
    {
        StartCoroutine(RecordFrame());
    }

    private IEnumerator ComputeMesh()
    {
        MeshCreator meshCreator = new MeshCreator(VoxelSet.GetBigPoints().ToArray());
        int[] triangles = meshCreator.ComputeMeshTriangles(radius: radius);

        yield return null;

        DrawMesh(triangles);
    }

    private void DrawMesh(int[] triangles)
    {
        Mesh mesh = new Mesh
        {
            vertices = VoxelSet.PointCloud(),
            triangles = triangles
        };

       // mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (this.meshObject != null)
            Destroy(this.meshObject);

        meshObject = new GameObject("ConvexHull", typeof(MeshFilter), typeof(MeshRenderer));
        meshObject.transform.localScale = new Vector3(1, 1, 1);
        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = meshMaterials[currentMaterial];
        meshObject.AddComponent<MeshCollider>();
        meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        meshObject.GetComponent<MeshCollider>().convex = false;
    }

    private void CloudUpdate(ARPointCloudChangedEventArgs obj)
    {
        if (!(Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Stationary))
            return;

        // here we use only the first element in array updated
        if (obj.updated.Count == 0)
            return;

        var updated = obj.updated[0];
        for (int i = 0; i < updated.positions.Length; ++i) {
            Vector3 screenPos = Camera.current.WorldToScreenPoint(updated.positions[i]);
            VoxelSet.AddPoint(updated.identifiers[i], updated.positions[i], updated.confidenceValues[i], Camera.current.transform.forward, true);
        }

        if (particlesShown)
            cloudSystem.SetParticles(VoxelSet.GetParticles().ToArray());
        if (useRealColors)
            UpdateColors();
        if (online)
            Mesh();
            

        GlobalVars.particles = VoxelSet.GetParticles();
        UpdateDebugInfo();
        if (drawVoxels)
           DrawVoxels();
    }

    private void UpdateDebugInfo()
    {
        debugText.text = $"Radius: {radiusSlider.value}\nVoxel Size: {voxelSizeSlider.value}\nParticles: {VoxelSet.GetParticles().Count}\nHidden Pa-s: {VoxelSet.GetPoints().Count}";
    }
}
