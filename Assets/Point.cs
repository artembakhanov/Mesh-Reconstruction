using UnityEngine;
using UnityEditor;

public class Point
{
    public Color Color = new Color(0, 255, 32);
    public ulong Id;
    public Vector3 Position;
    public Vector3 CameraDirection;
    public float ConfidenceValue;
    public float ColliderRadius = 0.01f;
    public int PointGroup;

    public Point(ulong id, Vector3 position, float confidenceValue)
    {
        ColliderRadius = VoxelSet.ColliderRadius;
        Id = id;
        Position = position;
        ConfidenceValue = confidenceValue;
    }
    
    public Point(ulong id, Vector3 position, float confidenceValue, Vector3 cameraDirection) : this (id, position, confidenceValue)
    {
        CameraDirection = cameraDirection;
    }

    public Point(ulong id, Vector3 position, float confidenceValue, Color color) : this(id, position, confidenceValue) {
        Color = color;
    }

    public Point(ulong id, Vector3 position, float confidenceValue, float colliderRadius) : this(id, position, confidenceValue)
    {
        ColliderRadius = colliderRadius;
    }

    public Point Copy()
    {
        return new Point(Id, Position, ConfidenceValue, CameraDirection);
    }
}