using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Used for creating influence region of an edge
/// </summary>
public class ConvexPolyhedron
{
    /// <summary>
    /// List of faces of the polyhedron
    /// </summary>
    public List<Face> Faces { get; private set; }
    
    public ConvexPolyhedron()
    {
        Faces = new List<Face>();
    }

    /// <summary>
    /// Add new face to the polyhedron
    /// </summary>
    /// <param name="normal">The normal vector of the face</param>
    /// <param name="point">The point that belongs to the face</param>
    public void AddFace(Vector3 normal, Vector3 point)
    {
        Faces.Add(new Face(normal, point));
    }

    /// <summary>
    /// Check whether the point is inside the polyhedron 
    /// </summary>
    /// <param name="position">The position of the point</param>
    /// <returns>
    /// True, if it is inside (also if it belongs to a face
    /// False, otherwise
    /// </returns>
    public bool IsPointInside(Vector3 position)
    {
        int lastSign = -2;
        foreach (var face in Faces)
        {
            int curSign = face.GetRelativePosition(position);
            if (curSign == 0)
                continue;
            if (lastSign == -2 || lastSign == curSign)
                lastSign = curSign;
            else
                return false;
        }

        return true;
    }
}
