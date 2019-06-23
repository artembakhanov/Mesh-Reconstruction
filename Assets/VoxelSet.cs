using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelSet
{
    public static float ColliderRadius {
        get { return VoxelSize / 2f; }
    }
    private Dictionary<Vector3Int, List<int>> Voxels;
    private Dictionary<ulong, int> IdIndexPairs;
    private List<Point> Points;
    private List<List<Point>> PointGroups;
    private List<ParticleSystem.Particle> Particles;
    private readonly float ParticleSize = 0.02f;
    private static float VoxelSize = 0.02f; // the size of a voxel is equal to the maximum diameter of a collider
    private ulong Counter = ulong.MaxValue; // used as id for new particles

    /// <summary>
    /// Default constructor
    /// </summary>
    public VoxelSet()
    {
        Voxels = new Dictionary<Vector3Int, List<int>>();
        Points = new List<Point>();
        PointGroups = new List<List<Point>>();
        IdIndexPairs = new Dictionary<ulong, int>();
        Particles = new List<ParticleSystem.Particle>();
    }

    /// <summary>
    /// Constructor with voxelSize parameter
    /// </summary>
    /// <param name="voxelSize">The size of each edge of every voxel</param>
    public VoxelSet(float voxelSize) : this()
    {
        VoxelSize = voxelSize;
    }

    public List<ParticleSystem.Particle> GetParticles() => Particles;

    public List<Point> GetPoints() => Points;

    public List<Point> GetBigPoints()
    {
        List<Point> bigPoints = new List<Point>(Points.Count);
        foreach (var point in Points)
        {
            var newPoint = point.Copy();
            newPoint.Position *= 100;
            bigPoints.Add(newPoint);
        }

        return bigPoints;
    }

    public Dictionary<Vector3Int, List<int>> GetVoxels() => Voxels;
        
    /// <summary>
    /// Clear voxel set
    /// </summary>
    public void Clear()
    {
        IdIndexPairs.Clear();
        Points = new List<Point>();
        Particles = new List<ParticleSystem.Particle>();
        Voxels = new Dictionary<Vector3Int, List<int>>();
    }

    /// <summary>
    /// Remove the given point
    /// </summary>
    /// <param name="point">Point to be removed</param>
    /// <returns>True if remove is successful. False otherwise</returns>
    public bool RemovePoint(Point point)
    {
        return RemovePoint(point.Id);    
    }

    /// <summary>
    /// Remove point by its id
    /// </summary>
    /// <param name="id">Id of the point to be removed</param>
    /// <returns>True if remove is successful. False otherwise</returns>
    public bool RemovePoint(ulong id)
    {
        if (!IdIndexPairs.TryGetValue(id, out int index))
            return false;

        GetVoxel(Points[index].Position).Remove(index);

        int lastIndex = Points.Count - 1;
        if (lastIndex > 0 && lastIndex != index)
        {
            List<int> voxel = GetVoxel(Points[lastIndex].Position);
            
            voxel[voxel.FindIndex(i => i.Equals(lastIndex))] = index;
            IdIndexPairs[Points[lastIndex].Id] = index;
            Points[index] = Points[lastIndex];
            Particles[index] = Particles[lastIndex];    
        }
        
        IdIndexPairs.Remove(id);
        Points.RemoveAt(lastIndex);
        Particles.RemoveAt(lastIndex);
        
        return true;
    }

    /// <summary>
    /// Add a point to voxel set
    /// </summary>
    /// <param name="id">Id of the point</param>
    /// <param name="position">Position of the point</param>
    /// <param name="confidenceValue">Confidence value of the point</param>
    /// <param name="checkCol">If true all possible collisions are removed</param>
    /// <returns>True if the point is added succesfully. If any collision is found return false</returns>
    public bool AddPoint(ulong id, Vector3 position, float confidenceValue, Vector3 cameraDirection, bool checkCol)
    {
        return AddPoint(new Point(id, position, confidenceValue, cameraDirection), checkCol);
    }

    /// <summary>
    /// Add a point with given color to voxel set
    /// </summary>
    /// <param name="id">Id of the point</param>
    /// <param name="position">Position of the point</param>
    /// <param name="confidenceValue">Confidence value of the point</param>
    /// <param name="color">The color of the point</param>
    /// <param name="checkCol">If true all possible collisions are removed</param>
    /// <returns>True if the point is added succesfully. If any collision is found return false</returns>
    public bool AddPoint(ulong id, Vector3 position, float confidenceValue, Color color, bool checkCol)
    {
        return AddPoint(new Point(id, position, confidenceValue, color), checkCol);
    }                                   

    /// <summary>
    /// Add a point to voxel set
    /// </summary>
    /// <param name="point">Point to be added</param>
    /// <param name="checkCol">If true all possible collisions are removed</param>
    /// <returns>True if the point is added succesfully. If any collision is found return false</returns>
    public bool AddPoint(Point point, bool checkCol)
    {
        if (IdIndexPairs.ContainsKey(point.Id)) return false;

        GetVoxel(point.Position).Add(Points.Count);
        IdIndexPairs.Add(point.Id, Points.Count);

        ParticleSystem.Particle particle = new ParticleSystem.Particle
        {
            startColor = point.Color,
            startSize = ParticleSize,
            position = point.Position,
            remainingLifetime = 10000f
        };
        Particles.Add(particle);

        Points.Add(point);
        if (checkCol)
            return RemoveCollisions(point);
        return true;
    }

    /// <summary>
    /// Change the size of voxel set
    /// </summary>
    /// <param name="newSize">New size of voxel set</param>
    public void ChangeSize(float newSize)
    {
        var points = Points;

        VoxelSize = newSize;
        IdIndexPairs.Clear();
        Points = new List<Point>();
        Particles = new List<ParticleSystem.Particle>();
        Voxels = new Dictionary<Vector3Int, List<int>>();

        foreach (var point in points)
        {
            point.ColliderRadius = ColliderRadius;
            AddPoint(point, true);
        }
    }

    public Vector3[] BigPointCloud()
    {
        Vector3[] pointCloud = new Vector3[Points.Count];

        for (int i = 0; i < Points.Count; i++)
        {
            Point point = (Point)Points[i];
            pointCloud[i] = point.Position * 100;
        }

        return pointCloud;
    }

    public Vector3[] PointCloud()
    {
        Vector3[] pointCloud = new Vector3[Points.Count];

        for (int i = 0; i < Points.Count; i++)
        {
            Point point = (Point)Points[i];
            pointCloud[i] = point.Position;
        }

        return pointCloud;
    }
    
    private bool RemoveCollisions(Point point)
    {
        var points = GetNearPoints(point.Position).ToArray();
        foreach (var current in points)
        {
            var d = Vector3.Distance(current.Position, point.Position);
            // If collision test succedes then we must update our 'average' point.
            if (d != 0 && d <= point.ColliderRadius + current.ColliderRadius)
            {                                           
                RemovePoint(point.Id);
                RemovePoint(current.Id);
                AddPoint(Counter, GetNewPosition(point, current), (point.ConfidenceValue + current.ConfidenceValue) / 2f, point.CameraDirection, true);
                Counter--;
                return false;
            }
        }

        return true;
    }

    private Vector3 GetNewPosition(Point p1, Point p2)
    {
        if (p1.ConfidenceValue != 0 && p2.ConfidenceValue != 0)
            return new Vector3(
                (p1.Position.x * p1.ConfidenceValue + p2.Position.x * p2.ConfidenceValue),
                (p1.Position.y * p1.ConfidenceValue + p2.Position.y * p2.ConfidenceValue),
                (p1.Position.z * p1.ConfidenceValue + p2.Position.z * p2.ConfidenceValue)
                ) / (p1.ConfidenceValue + p2.ConfidenceValue);
        else
            return (p1.Position + p2.Position) / 2f;
    }

    private List<Point> GetNearPoints(Vector3 position)
    {
        List<Vector3Int> nearVoxels = GetNearVoxels(position);
        List<Point> points = new List<Point>();

        foreach (Vector3Int voxel in nearVoxels)
        {
            List<int> vPoints = GetVoxel(voxel);
            foreach (int point in vPoints)
            {
                points.Add(Points[point]);
            }
        }

        return points;
    }

    /// <summary>
    /// Returns all keys of voxels, in which collision is possible
    /// for collider with the given position.
    /// The size is always 8
    /// </summary>
    /// <param name="position">Position of the collider</param>
    /// <returns>List of keys to corresponding voxels</returns>
    private List<Vector3Int> GetNearVoxels(Vector3 position)
    {
        Vector3Int key = GetKey(position);
        Vector3Int direction = GetDirection(position);
        List<Vector3Int> voxels = new List<Vector3Int>(8);

        for (int i = 0; i < 8; ++i)
        {
            Vector3Int voxel = new Vector3Int
            {
                x = (i & 1) * direction.x,
                y = ((i & 2) != 0 ? 1 : 0) * direction.y,
                z = ((i & 4) != 0 ? 1 : 0) * direction.z,
            };

            voxels.Add(key + voxel);
        }

        return voxels;
    }

    private Vector3Int GetDirection(Vector3 position)
    {
        position.x = position.x % VoxelSize;
        position.y = position.y % VoxelSize;
        position.z = position.z % VoxelSize;

        if (position.x < 0) position.x += VoxelSize;
        if (position.y < 0) position.y += VoxelSize;
        if (position.z < 0) position.z += VoxelSize;

        position.x -= VoxelSize / 2;
        position.y -= VoxelSize / 2;
        position.z -= VoxelSize / 2;

        Vector3Int direction = new Vector3Int
        {
            x = position.x > 0 ? 1 : -1,
            y = position.y > 0 ? 1 : -1,
            z = position.z > 0 ? 1 : -1
        };
        
        return direction;
    }

    private List<int> GetVoxel(Vector3 position)
    {
        return GetVoxel(GetKey(position));
    }

    private List<int> GetVoxel(Vector3Int key)
    {
        if (!Voxels.ContainsKey(key))
        {
            Voxels.Add(key, new List<int>());
        }

        return Voxels[key];
    }

    private Vector3Int GetKey(Vector3 position)
    {
        return new Vector3Int((int) (position.x / VoxelSize), (int) (position.y / VoxelSize), (int) (position.z / VoxelSize));
    }
}
