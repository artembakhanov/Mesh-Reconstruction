using UnityEngine;
using UnityEditor;
using System;

public class Face
{
    public readonly Vector3 point;
    public readonly Vector3 normal;
    public readonly float D;

    public Face(Vector3 normal, Vector3 point)
    {
        this.point = point;
        this.normal = normal;
        this.D = -Vector3.Dot(normal, point);
    }

    /// <summary>
    /// Get the relative position of the point
    /// </summary>
    /// <param name="point">The point, which relative position is to be found</param>
    /// <returns>
    /// -1, if the point if on the positive direction (radius vector has the same direction as the normal vector does)
    /// 1, otherwise
    /// </returns>    
    public int GetRelativePosition(Vector3 point)
    {
        float pos = Vector3.Dot(point, normal) + D;
        if (pos < 1e-7f && pos > -1e-7f) return 0;
        else return Math.Sign(pos);
    }
}