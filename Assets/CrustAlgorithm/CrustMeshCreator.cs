using Bakhanov.VoxelSet;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// This is the implementain of Crust algorithm.    
/// <see cref="https://github.com/ricebean-net/PointCloudCrust"/>
/// </summary>
public class CrustMeshCreator
{
    private Point[] pointCloud;
    private List<Triangle> triangles;
    private List<int> meshTriangles;
    private float radius;
    private float maxDistance = 20f;

    public CrustMeshCreator(Point[] pointCloud)
    {   
        this.pointCloud = pointCloud;
    }

    public int[] ComputeMeshTriangles(float radius = 20f)
    {
        ComputeCrustTriangles(radius);

        return meshTriangles.ToArray();
    }

    public List<Triangle> ComputeCrustTriangles(float radius = 20f)
    {
        meshTriangles = new List<int>();
        triangles = new List<Triangle>();
        this.radius = radius;

        Parallel.For(0, pointCloud.Length, ComputeTriangle);

        return null;
    }

    private void ComputeTriangle(int corner1)
    {
        for (int corner2 = corner1 + 1; corner2 < pointCloud.Length; ++corner2)
        {
            for (int corner3 = corner2 + 1; corner3 < pointCloud.Length; ++corner3)
            {
                Vector3[] triangle = new Vector3[]{
                    pointCloud[corner1].Position,
                    pointCloud[corner2].Position,
                    pointCloud[corner3].Position
                };

                Vector3? triangleCenter = TriangleCenter(triangle);

                if (triangleCenter != null)
                {
                    Vector3[] ballCenters = BallCenter(radius, triangle, (Vector3) triangleCenter);

                    if (ballCenters != null)
                    {
                        bool keep = AnalyzeTriangle(triangle, ballCenters, radius, pointCloud);

                        if (keep)
                        {
                            AddTriangle(corner1, corner2, corner3);
                        }
                    }
                }
            }
        }
    }

    private void AddTriangle(int corner1, int corner2, int corner3)
    {
        Vector3 normal = Vector3.Cross(pointCloud[corner2].Position - pointCloud[corner1].Position, pointCloud[corner3].Position - pointCloud[corner1].Position);
        Vector3 direction = pointCloud[corner1].CameraDirection + pointCloud[corner2].CameraDirection + pointCloud[corner3].CameraDirection;

        float dot = Vector3.Dot(normal, direction);

        meshTriangles.Add(corner1);
        if (dot >= 0)
        {
            triangles.Add(new Triangle(corner1, corner3, corner2));
            meshTriangles.Add(corner3);
            meshTriangles.Add(corner2);
        } else
        {
            triangles.Add(new Triangle(corner1, corner2, corner3));
            meshTriangles.Add(corner2);
            meshTriangles.Add(corner3);
        }
    }

    private Vector3? TriangleCenter(Vector3[] triangle)
    {
        Vector3 ac = triangle[2] - triangle[0];
        Vector3 ab = triangle[1] - triangle[0];

        Vector3 abXac = Vector3.Cross(ab, ac);

        if (abXac.magnitude == 0)
            return null;

        Vector3 v1 = Vector3.Cross(abXac, ab) * ac.sqrMagnitude;
        Vector3 v2 = Vector3.Cross(ac, abXac) * ab.sqrMagnitude;

        float f = 2f * abXac.sqrMagnitude;

        Vector3 v = (v1 + v2) / f;
        return triangle[0] + v;
    }

    private Vector3[] BallCenter(float radius, Vector3[] triangle, Vector3 triangleCenter)
    {
        Vector3 am = triangleCenter - triangle[0];

        float amLen = am.magnitude;

        if (amLen > radius)
            return null;

        float mhLen = Mathf.Sqrt(radius * radius - amLen * amLen);

        Vector3 ab = triangle[1] - triangle[0];
        Vector3 orth = Vector3.Cross(am, ab);

        Vector3 mhNorm = orth.normalized;

        Vector3 mh1 = mhNorm * mhLen;
        Vector3 mh2 = mhNorm * mhLen * -1;

        return new Vector3[]
        {
            triangleCenter + mh1,
            triangleCenter + mh2,
        };
    }

    private bool AnalyzeTriangle(Vector3[] triangle, Vector3[] ballCenters, float radius, Point[] pointCloud)
    {
        float tolerance = 0.01f;
        Vector3 ball;

        bool keep = true;

        ball = ballCenters[0];

        if (Vector3.Distance(triangle[0], triangle[1]) > maxDistance || Vector3.Distance(triangle[0], triangle[2]) > maxDistance || Vector3.Distance(triangle[1], triangle[2]) > maxDistance)
            return false;

        for (int i = 0; i < pointCloud.Length && keep; ++i)
        {
            if ((ball - pointCloud[i].Position).magnitude < radius - tolerance)
            {
                keep = false;
            }
        }

        if (!keep)
        {
            keep = true;

            ball = ballCenters[1];

            for (int i = 0; i < pointCloud.Length && keep; ++i)
            {
                if ((ball - pointCloud[i].Position).magnitude < radius - tolerance)
                {
                    keep = false;
                }
            }
        }

        return keep;
    }
}
