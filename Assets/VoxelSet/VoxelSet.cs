using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Bakhanov.VoxelSet;
using System;

public class VoxelSet
{
    public delegate void UpdateHandler();
    public delegate void NewActivePointsHandler(NewPointsArgs e);
    public event NewActivePointsHandler NewActivePointsEvent;
    public event UpdateHandler UpdateEvent;

    public Dictionary<Vector3Int, List<int>> Voxels;
    private Dictionary<Vector3Int, int> VoxelVersions;
    private Dictionary<ulong, int> IdIndexPairs;
    public List<Point> Points;
    public int version
    {
        get
        {
            return Version / SmartUpdateIterations;
        }
    }
    private int Version = 0;
    private static float VoxelSize = 0.05f;
    private static ulong Counter = ulong.MaxValue;
    private List<Point> newActivePoints = new List<Point>(); // right-handed positions
    public List<int> lastActivePoints = new List<int>(); // right-handed positions
    public static float MaxColliderRadius
    {
        get { return VoxelSize / 2f; }
    }

    public static float MinColliderRadius
    {
        get { return MaxColliderRadius / 4f; }
    }

    public bool CheckRadix = true;
    public bool SmartUpdate = false;
    public int SmartUpdateIterations = 1;

    public VoxelSet()
    {
        Voxels = new Dictionary<Vector3Int, List<int>>();
        Points = new List<Point>();
        IdIndexPairs = new Dictionary<ulong, int>();
        VoxelVersions = new Dictionary<Vector3Int, int>();
    }

    public VoxelSet(float voxelSize) : this()
    {
        VoxelSet.VoxelSize = voxelSize;
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
        Voxels = new Dictionary<Vector3Int, List<int>>();
        VoxelVersions = new Dictionary<Vector3Int, int>();

        foreach (var point in points)
        {
            point.ColliderRadius = Radius(point.ConfidenceValue);
            AddPoint(point, true);
        }
    }

    public Vector3[] Vertices()
    {
        Vector3[] vertices = new Vector3[Points.Count];

        for (int i = 0; i < Points.Count; i++)
        {
            vertices[i] = Points[i].Position;
        }

        return vertices;
    }
    
    #region Add
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
        return AddPoint(new Point(id, position, confidenceValue, cameraDirection, version), checkCol);
    }

    /// <summary>
    /// Add a point to voxel set
    /// </summary>
    /// <param name="point">Point to be added</param>
    /// <param name="checkCol">If true all possible collisions are removed</param>
    /// <returns>True if the point is added succesfully. If any collision is found return false</returns>
    public bool AddPoint(Point point, bool checkCol)
    {
        if (IdIndexPairs.ContainsKey(point.Id))
        {
            point.Id = Counter;
            --Counter;
        }

        if (CheckRadix)
            point.ColliderRadius = Radius(point.ConfidenceValue);
        else
            point.ColliderRadius = Radius(0);

        if (!SmartUpdate || CheckVoxelVersion(GetKey(point.Position)))
        {
            GetVoxelForEditing(point.Position).Add(Points.Count);
            IdIndexPairs.Add(point.Id, Points.Count);
            Points.Add(point);
            if (checkCol)
                return RemoveCollisions(point);
            else
                newActivePoints.Add(point);
        }
       // else if (VoxelVersions[GetKey(point.Position)] == version)
         //   newActivePoints.Add(point.RightHandedPosition);
        return true;
    }
    #endregion 

    #region Remove
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

        var key = GetKey(Points[index].Position);
        var vox = GetVoxelForEditing(key);
        //if (vox.Count == 1)
        //{
        //    Voxels.Remove(GetKey(Points[index].Position));
        //    VoxelVersions.Remove(GetKey(Points[index].Position));
        //}
        //else
        vox.Remove(index);

        int lastIndex = Points.Count - 1;
        if (lastIndex > 0 && lastIndex != index)
        {
            List<int> voxel = GetVoxelForEditing(Points[lastIndex].Position);

            voxel[voxel.FindIndex(i => i.Equals(lastIndex))] = index;
            IdIndexPairs[Points[lastIndex].Id] = index;
            Points[index] = Points[lastIndex];
        }

        IdIndexPairs.Remove(id);
        Points.RemoveAt(lastIndex);
        if (vox.Count == 0)
        {
            Voxels.Remove(key);
            //Voxels.Remove(GetKey(Points[index].Position));
            VoxelVersions.Remove(key);
        }
        return true;
    }
    #endregion

    public void Update(int smartUpdateIterations = 1)
    {
        int prevVersion = version;
        List<int> newActivePoints = new List<int>();
        for (var i = 0; i < Points.Count; ++i)
        {
            if (version == Points[i].Version)
            {
                newActivePoints.Add(i);
            }
        }
        Version++;

        if (!SmartUpdate || prevVersion != version)
        {
            lastActivePoints = newActivePoints;
            NewActivePointsEvent?.Invoke(new NewPointsArgs(newActivePoints));
            UpdateEvent?.Invoke();
        }

        int counter = 0;
        int overall = 0;
        foreach (var voxel in Voxels)
        {
            counter++;  
            overall += voxel.Value.Count;
        }

        Debug.Log($"voxel set update {Points.Count}");
        //Debug.Log($"{(float) overall / counter}");
    }

    /// <summary>
    /// Clear voxel set
    /// </summary>
    public void Clear()
    {
        IdIndexPairs.Clear();
        Points = new List<Point>();
        Voxels = new Dictionary<Vector3Int, List<int>>();
    }

    /// <summary>
    /// Get points that are inside a polyhedron.
    /// </summary>
    /// <param name="p">The polyhedron.</param>
    /// <returns>The indices of the points.</returns>
    public List<int> GetInnerPoints(ConvexPolyhedron p, bool smartUpdate, bool influenceRegion2, Vector3? seedPoint = null)
    {
        List<int> points = new List<int>();

        if (seedPoint != null)
        {
            if (false)
            {
                for (int k = 0; k < lastActivePoints.Count; ++k)
                {
                    if (p.IsPointInside(Points[lastActivePoints[k]].RightHandedPosition, influenceRegion2))
                    {
                        points.Add(lastActivePoints[k]);
                    }
                }
            }
            else
            {
                for (int k = 0; k < Points.Count; ++k)
                {
                    if (p.IsPointInside(Points[k].RightHandedPosition, influenceRegion2))
                        points.Add(k);
                }
            }
        }
        else
        {
            Vector3 _seedPoint = (Vector3)seedPoint;
            Vector3Int seedKey = GetKey(new Vector3(_seedPoint.x, _seedPoint.z, _seedPoint.y));
            for (int i = -2; i <= 2; ++i)
            {
                for (int j = -2; j <= 2; ++j)
                {
                    for (int l = -2; l <= 2; ++l)
                    {
                        var voxelPoints = GetVoxelForEditing(seedKey + new Vector3Int(i, j, l));
                        for (int k = 0; k < voxelPoints.Count; ++k)
                        {
                            if (p.IsPointInside(Points[voxelPoints[k]].RightHandedPosition.normalized, influenceRegion2))
                                points.Add(voxelPoints[k]);
                        }
                    }
                }
            }
        }

        return points;
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
                if (!SmartUpdate ||
                    CheckVoxelVersion(GetKey(point.Position))
                    && CheckVoxelVersion(GetKey(current.Position))
                    && CheckVoxelVersion(GetKey(GetNewPosition(point, current))))
                    {
                        RemovePoint(current.Id);
                        AddPoint(Counter, GetNewPosition(point, current), (point.ConfidenceValue + current.ConfidenceValue) / 2f, point.CameraDirection, true);
                        Counter--;
                        return false;
                    }
            }
        }

        if (CheckVoxelVersion(GetKey(point.Position)))
            newActivePoints.Add(point);

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

    /// <summary>
    /// Return radius of influnce region of a point in point cloud
    /// Used for decreasing the number of particles
    /// </summary>
    /// <param name="confidenceValue">Confidence value of a point in point cloud</param>
    /// <returns>Radius of influnce region</returns>
    private static float Radius(float confidenceValue)
    {
        return MaxColliderRadius - (MaxColliderRadius - MinColliderRadius) * confidenceValue;
    }

    private List<int> GetVoxelForEditing(Vector3 position)
    {
        return GetVoxelForEditing(GetKey(position));
    }

    private List<int> GetVoxelForEditing(Vector3Int key)
    {
        if (!Voxels.ContainsKey(key))
        {
            Voxels.Add(key, new List<int>());
            VoxelVersions.Add(key, version);
        }
        
        return Voxels[key];
    }

    private List<int> GetVoxel(Vector3Int key)
    {
        if (!Voxels.ContainsKey(key))
        {
            return new List<int>();
        }

        return Voxels[key];
    }

    private Vector3Int GetKey(Vector3 position)
    {
        return new Vector3Int((int)Math.Floor(position.x / VoxelSize), (int)Math.Floor(position.y / VoxelSize), (int)Math.Floor(position.z / VoxelSize));
    }

    private bool CheckVoxelVersion(Vector3Int key)
    {
        if (VoxelVersions.TryGetValue(key, out int voxelVersion))
            return voxelVersion == version;
        else
            return true;
    }
}