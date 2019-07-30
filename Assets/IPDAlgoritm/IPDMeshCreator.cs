using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Bakhanov.IPD;
using System;
using System.IO;

public class IPDMeshCreator
{
    public bool influenceRegion2 = false;
    public bool energyFunction2 = false;
    public bool smartUpdate = false;
    public bool closedInfluenceRegion2 = true;
    public bool newNormalByDot = false;
    public bool checkLengths = true;
    public float maxEdgeLength = 0.1714f;
    public bool checkFacesAngle = true;
    public float sMult = 1f;
    private int buildPCounter = 0;
    private int findAPCounter = 0;

    private List<Point> PointCloud = new List<Point>();
    private VoxelSet VoxelSet;
    private List<int> meshTriangles = new List<int>();

    Queue<Edge> aeq = new Queue<Edge>(); // active-edge queue
    Dictionary<Edge, Edge> edges = new Dictionary<Edge, Edge>();
    HashSet<Edge> fixedEdges = new HashSet<Edge>();
    HashSet<Edge> boundaryEdges = new HashSet<Edge>();
    HashSet<int> fixedVertices = new HashSet<int>();
    List<Triangle> triangles = new List<Triangle>();
    List<int> newActivePoints = new List<int>();
    private float regionAngle;
    

    public IPDMeshCreator(VoxelSet voxelSet)
    {
        VoxelSet = voxelSet;
    }

    public IPDMeshCreator(VoxelSet voxelSet, bool smartUpdate) : this(voxelSet)
    {
        this.smartUpdate = smartUpdate;
    }

    public IPDMeshCreator(VoxelSet voxelSet, bool smartUpdate, bool influenceRegion2) : this(voxelSet, smartUpdate)
    {
        this.influenceRegion2 = influenceRegion2;
    }

    public IPDMeshCreator(VoxelSet voxelSet, bool smartUpdate, bool influenceRegion2, float regionAngle) : this(voxelSet, smartUpdate, influenceRegion2)
    {
        this.regionAngle = regionAngle;
    }

    public IPDMeshCreator(VoxelSet voxelSet, bool smartUpdate, bool influenceRegion2, bool energyFunction2, float regionAngle) : this(voxelSet, smartUpdate, influenceRegion2)
    {
        this.energyFunction2 = energyFunction2;
        this.regionAngle = regionAngle;
    }


    /// <summary>
    /// Generate all triangles for the point cloud.
    /// </summary>
    /// <returns>Array of triangles that is used by standard unity mesh renderer.</returns>
    public int[] ComputeMeshTriangles(bool forceUpdate = false) 
    {
        if (forceUpdate)
        {
            PointCloud = new List<Point>();
            var temp = meshTriangles;
            meshTriangles = new List<int>();
            aeq = new Queue<Edge>(); // active-edge queue
            edges = new Dictionary<Edge, Edge>();
            fixedEdges = new HashSet<Edge>();
            boundaryEdges = new HashSet<Edge>();
            fixedVertices = new HashSet<int>();
            triangles = new List<Triangle>();
            foreach (var point in VoxelSet.Points)
            {
                PointCloud.Add(new Point(point.RightHandedPosition));
            }
            AddSeedTriangle(temp.ToArray());
        }
        else if (!smartUpdate)   
        {
            PointCloud = new List<Point>();
            meshTriangles = new List<int>();
            aeq = new Queue<Edge>(); // active-edge queue
            edges = new Dictionary<Edge, Edge>();
            fixedEdges = new HashSet<Edge>();
            boundaryEdges = new HashSet<Edge>();
            fixedVertices = new HashSet<int>();
            triangles = new List<Triangle>();
            foreach (var point in VoxelSet.Points)
            {
                PointCloud.Add(new Point(point.RightHandedPosition));
            }
        } else
        {
            var temp = PointCloud;
            var lcount = VoxelSet.lastActivePoints.Count;

            for (int i = 0; i < temp.Count; i++)
            {
                Point p = temp[i];
                if (p.status == PointStatus.ACTIVE)
                {
                    newActivePoints.Add(i);
                    VoxelSet.lastActivePoints.Add(i);
                }
            }

            var points = VoxelSet.Points;
            for (int i = 0; i < lcount; ++i)
            {
                PointCloud.Add(new Point(points[VoxelSet.lastActivePoints[i]].RightHandedPosition));
                newActivePoints.Add(VoxelSet.lastActivePoints[i]);
            }

            foreach (var edge in boundaryEdges)
            {
                aeq.Enqueue(edge);
            }

            boundaryEdges.Clear();
        }

        if (PointCloud.Count < 3) return meshTriangles.ToArray(); //throw new System.Exception("Impossible to generate mesh with less than 3 points!");


        Debug.Log("Here start searching a seed triangle");
        if (meshTriangles.Count == 0)
            GenerateSeedTriangle(aeq, edges, triangles); // add first triangle to the mesh

        Debug.Log("Here start searching other points");
        while (aeq.Count != 0)
        {
            Edge edge_ij = aeq.Dequeue(); // current edge
            if (fixedEdges.Contains(edge_ij) || boundaryEdges.Contains(edge_ij)) continue; // if it is not active
            int? activePoint = FindActivePoint(edge_ij, edges, triangles, fixedEdges, fixedVertices); // find the best active point 
            if (activePoint != null)
            {
                AddTriangle(edge_ij, (int)activePoint, aeq, edges, fixedEdges, triangles, fixedVertices);
            } else
            {
                boundaryEdges.Add(edge_ij);
            }
        }

        Debug.Log($"Triangle check 3d { Triangle.counter3d }, Triangle check 2d { Triangle.counter2d }, find active point {findAPCounter}, build polyhedron {buildPCounter}");
        return meshTriangles.ToArray();
    }

    private void SaveInfo(Queue<Edge> aeq, Dictionary<Edge, Edge> edges, HashSet<Edge> fixedEdges)
    {
        var points = VoxelSet.Points;
        string filePath = Application.persistentDataPath + "/mesh.txt";
        string filePath0 = Application.persistentDataPath + "/meshPoints.txt";
        if (!File.Exists(filePath))
            File.Create(filePath);

        if (!File.Exists(filePath0))
            File.Create(filePath0);

        List<string> lines = new List<string>();
        List<string> lines0 = new List<string>();

        lines.Add("-----------Points-----------");
        for (int i = 0; i < PointCloud.Count; i++)
        {
            var p = PointCloud[i];
            var p0 = points[i];
            lines.Add($"{i} st {p.status} me {p.MinEdge} mx {p.MaxEdge} ud {p.UniformityDegree} pos {p.Position}");
            lines0.Add($"{p0.Position.x} {p0.Position.y} {p0.Position.z}");
        }

        lines.Add("-----------Edges-----------");
        foreach (var edge in edges)
        {
            //lines.Add($"v1 {edge.vertex1} v2 {edge.vertex2} v3 {edge.vertex3} l {edge.length} n {edge.normal} s {edge.status}");
        }

        lines.Add("-----------Fixed Edges-----------");
        foreach (var edge in fixedEdges)
        {
            lines.Add($"v1 {edge.vertex1} v2 {edge.vertex2} v3 {edge.vertex3} l {edge.length} n {edge.normal} s {edge.status}");
        }
        File.WriteAllLines(filePath, lines.ToArray());
        File.WriteAllLines(filePath0, lines0.ToArray());
        
    }

    /// <summary>
    /// Build an influence region of the given edge.
    /// </summary>
    /// <param name="edge">The edge, which influence region to be foind.</param>
    /// <param name="edges">The set of existing edges.</param>
    /// <returns>Polyhedron object that is the influence region.</returns>
    private ConvexPolyhedron BuildPolyhedron(Edge edge, Dictionary<Edge, Edge> edges, out int ap1, out int ap2, out List<int> innerPoints)
    {
        if (influenceRegion2) return BuildPolyhedron2(edge, edges, out ap1, out ap2, out innerPoints);
        buildPCounter++; 
        ap1 = -1;
        ap2 = -1;
        int i = edge.vertex1;
        int j = edge.vertex2;
        int k = edge.vertex3;   
        
        // check if the i and j vertices are chosen correctly
        if (Vector3.Dot(Vector3.Cross(PointCloud[k].Position - PointCloud[i].Position, PointCloud[k].Position - PointCloud[j].Position), edge.normal) < 0)
        {
            i = edge.vertex2;
            j = edge.vertex1;
        }

        // positions of i, j and k
        Vector3 ip = PointCloud[i].Position;
        Vector3 jp = PointCloud[j].Position;
        Vector3 kp = PointCloud[k].Position;

        // the size of the polyhedron
        float s = (float) Math.Max(PointCloud[i].UniformityDegree, PointCloud[j].UniformityDegree) * (PointCloud[i].MinEdge + PointCloud[j].MinEdge) / 2;

        // bary center of the triangle ijk
        Vector3 p = (ip + jp + kp) / 3;
        // center of the edge ij
        Vector3 p_m = (ip + jp) / 2;
        // normal of the triangle (polygon) ijk
        Vector3 N = edge.normal;

        ConvexPolyhedron polyhedron = new ConvexPolyhedron();

        Vector3 n1 = Vector3.Cross(edge.normal, jp - ip).normalized;
        polyhedron.AddFace(n1, ip); // test

        polyhedron.AddFace(N, p_m + 2 * s * N); // top face
        polyhedron.AddFace(-N, p_m - 2 * s * N); // bottom face - 
        polyhedron.AddFace(-Vector3.Cross(N, ip - p).normalized, ip); // face containing pi - 
        polyhedron.AddFace(Vector3.Cross(N, jp - p).normalized, jp); // face containing pj  
        Vector3 N5 = Vector3.Cross(jp - ip, N).normalized;
        polyhedron.AddFace(N5, p_m + s * N5);

        // here it checks whether there are some edges inside the influnce region
        // that are connected to i or j
        var points = VoxelSet.GetInnerPoints(polyhedron, smartUpdate, influenceRegion2, p_m);
        Vector3? pLeft = null, pRight = null;
        //float minAngleLeft = 181f, minAngleRight = 181f;
        float minAngleLeft = 90 + regionAngle, minAngleRight = 90 + regionAngle;
        foreach (var point in points)
        {
            if (point == i || point == j) continue;
            Edge left = new Edge(point, i, Vector3.back);
            Edge right = new Edge(point, j, Vector3.back);

            if (edges.ContainsKey(left))
            {
                float angleLeft = Vector3.Angle(jp - ip, PointCloud[point].Position - ip);
                if (angleLeft < minAngleLeft)
                {
                    minAngleLeft = angleLeft;
                    pLeft = PointCloud[point].Position;
                    ap1 = point;
                }
            }
            if (edges.ContainsKey(right))
            {
                float angleRight = Vector3.Angle(ip - jp, PointCloud[point].Position - jp);
                if (angleRight < minAngleRight)
                {
                    minAngleRight = angleRight;
                    pRight = PointCloud[point].Position;
                    ap2 = point;
                }
            }
        }

        // additional faces that we need to hold geometry integrity
        if (pLeft != null)
        {
            polyhedron.AddFace(-Vector3.Cross(N, (Vector3)pLeft - ip).normalized, ip);
        }

        if (pRight != null)
        {
            polyhedron.AddFace(Vector3.Cross(N, (Vector3)pRight - jp).normalized, jp);
        }

        innerPoints = points;
        return polyhedron;
    }

    /// <summary>
    /// Build open influence region with 3 faces for a given edge.
    /// </summary>
    /// <param name="edge">The given edge.</param>
    /// <param name="edges">All existing edges.</param>
    /// <param name="ap1">Allowed point 1</param>
    /// <param name="ap2">Allowed point 2</param>s
    /// <param name="innerPoints">Points that approximately lie inside the influence region (polyhedron).</param>
    /// <returns>The polyhedron.</returns>
    private ConvexPolyhedron BuildPolyhedron2(Edge edge, Dictionary<Edge, Edge> edges, out int ap1, out int ap2, out List<int> innerPoints)
    {
        buildPCounter++;
        ap1 = -1;
        ap2 = -1;
        int i = edge.vertex1;
        int j = edge.vertex2;
        int k = edge.vertex3;

        // check if the i and j vertices are chosen correctly
        if (Vector3.Dot(Vector3.Cross(PointCloud[k].Position - PointCloud[i].Position, PointCloud[k].Position - PointCloud[j].Position), edge.normal) < 0)
        {
            i = edge.vertex2;
            j = edge.vertex1;
        }

        // positions of i, j and k
        Vector3 ip = PointCloud[i].Position;
        Vector3 jp = PointCloud[j].Position;
        Vector3 kp = PointCloud[k].Position;

        Vector3 n1 = Vector3.Cross(edge.normal, jp - ip).normalized;
        Vector3 n3 = (Vector3.Cross(n1 + Mathf.Tan(Mathf.Deg2Rad * regionAngle) * (ip - jp).normalized, edge.normal)).normalized;
        Vector3 n2 = Vector3.Cross(edge.normal, n1 + Mathf.Tan(Mathf.Deg2Rad * regionAngle) * (jp - ip).normalized).normalized;

        float s = sMult * (float)Math.Max(PointCloud[i].UniformityDegree, PointCloud[j].UniformityDegree) * (PointCloud[i].MinEdge + PointCloud[j].MinEdge) / 2;


        ConvexPolyhedron polyhedron = new ConvexPolyhedron();
        polyhedron.AddFace(n1, ip);
        polyhedron.AddFace(n2, ip);
        polyhedron.AddFace(n3, jp);

        polyhedron.AddFace(-n1, s * -n1 + (ip + jp) / 2);
        polyhedron.AddFace(edge.normal, ip + s * edge.normal); // top face
        polyhedron.AddFace(-edge.normal, ip - s * edge.normal); // bottom face - 


        innerPoints = VoxelSet.GetInnerPoints(polyhedron, false, influenceRegion2, (ip + jp) / 2);
        Vector3? pLeft = null, pRight = null;
        float minAngleLeft = 181f, minAngleRight = 181f;
        foreach (var point in innerPoints)
        {
            if (point == i || point == j) continue;
            Edge left = new Edge(point, i, Vector3.back);
            Edge right = new Edge(point, j, Vector3.back);

            if (edges.ContainsKey(left))
            {
                float angleLeft = Vector3.Angle(ip - jp, PointCloud[point].Position - ip);
                if (angleLeft < minAngleLeft)
                {
                    minAngleLeft = angleLeft;
                    pLeft = PointCloud[point].Position;
                    ap1 = point;
                }
            }
            if (edges.ContainsKey(right))
            {
                float angleRight = Vector3.Angle(jp - ip, PointCloud[point].Position - jp);
                if (angleRight < minAngleRight)
                {
                    minAngleRight = angleRight;
                    pRight = PointCloud[point].Position;
                    ap2 = point;
                }
            }
        }

        ConvexPolyhedron p = new ConvexPolyhedron();
        p.AddFace(n1, ip);

        if (pLeft == null)
            p.AddFace(n2, ip);
        else
            p.AddFace(Vector3.Cross((Vector3)pLeft - ip, edge.normal).normalized, ip);

        if (pRight == null)
            p.AddFace(n3, jp);
        else
            p.AddFace(-Vector3.Cross((Vector3)pRight - jp, edge.normal).normalized, jp);

        polyhedron.AddFace(-n1, s * -n1 + (ip + jp) / 2);

        return p;
    }

    /// <summary>
    /// Find the most appropriate active point for the given edge.
    /// </summary>
    /// <param name="e_ij">Edge</param>
    /// <param name="edges">Set of existing edges</param>
    /// <param name="triangles">List of existing triangles</param>
    /// <returns>null if there is no active point, the index of the point</returns>
    private int? FindActivePoint(Edge e_ij, Dictionary<Edge, Edge> edges, List<Triangle> triangles, HashSet<Edge> fixedEdges, HashSet<int> fixedVertices)
    {
        findAPCounter++;
        int ap1, ap2;

        List<int> innerPoints;
        ConvexPolyhedron p = BuildPolyhedron(e_ij, edges, out ap1, out ap2, out innerPoints);
        List<Vector3> allowedPoints = new List<Vector3>
        {
            PointCloud[e_ij.vertex1].Position,
            PointCloud[e_ij.vertex2].Position
        };
        if (ap1 != -1 && !fixedVertices.Contains(ap1)) allowedPoints.Add(PointCloud[ap1].Position);
        if (ap2 != -1 && !fixedVertices.Contains(ap2)) allowedPoints.Add(PointCloud[ap2].Position);

        //var points = VoxelSet.GetInnerPoints(p, (PointCloud[e_ij.vertex1].Position + PointCloud[e_ij.vertex2].Position) / 2);
        var points = GetInnerPoints(p, innerPoints);
        float sum = float.PositiveInfinity;
        int? activePoint = null;
        HashSet<Triangle> tr = new HashSet<Triangle>();
        foreach (var m in points)
        {
            tr.UnionWith(PointCloud[m].triangles);
        }
        foreach (var m in points)
        {
            float currentSum = CalculateEnergy(e_ij.vertex1, e_ij.vertex2, e_ij.vertex3, m);
            if (PointCloud[m].status == PointStatus.ACTIVE 
                && currentSum < sum 
                && (!checkLengths || Distance(m, e_ij.vertex1) <= maxEdgeLength && Distance(m, e_ij.vertex2) <= maxEdgeLength)
                && GeomIntegrity(e_ij, m, tr, allowedPoints.ToArray(), fixedEdges, fixedVertices)) // if do not intersect other triangles
            {
                activePoint = m;
                sum = currentSum;
            }
        }

        return activePoint;
    }

    private List<int> GetInnerPoints(ConvexPolyhedron p, List<int> innerPoints)
    {
        List<int> points = new List<int>();
        for (int k = 0; k < innerPoints.Count; ++k)
        {
            if (p.IsPointInside(PointCloud[innerPoints[k]].Position, influenceRegion2))
                points.Add(innerPoints[k]);
        }

        return points;
    }

    /// <summary>
    /// Check geometry integrity of mesh
    /// </summary>
    /// <param name="e_ij">Current edge</param>
    /// <param name="m">Candidate</param>
    /// <param name="triangles">List of existing triangles</param>
    /// <returns>true if geometry integrity holds, false otherwise</returns>
    private bool GeomIntegrity(Edge e_ij, int m, HashSet<Triangle> triangles, Vector3[] allowedPoints, HashSet<Edge> fixedEdges, HashSet<int> fixedVertices)
    {

        if (checkFacesAngle)
        {
            int i = e_ij.vertex1;
            int j = e_ij.vertex2;
            int k = e_ij.vertex3;

            // check if the i and j vertices are chosen correctly
            if (Vector3.Dot(Vector3.Cross(PointCloud[k].Position - PointCloud[i].Position, PointCloud[k].Position - PointCloud[j].Position), e_ij.normal) < 0)
            {
                i = e_ij.vertex2;
                j = e_ij.vertex1;
            }

            var normal = Vector3.Cross(PointCloud[m].Position - PointCloud[j].Position, PointCloud[m].Position - PointCloud[i].Position).normalized;

            int angles = 1;
            float anglesSum = 0;

            if (edges.TryGetValue(new Edge(i, m), out Edge edge1))
            {
                angles++; // a
                var angle1 = Vector3.Angle(normal, edge1.normal);
                anglesSum += angle1; // a

                //if (angle1 < 0 || angle1 > 45) return false;
            }

            if (edges.TryGetValue(new Edge(j, m), out Edge edge2))
            {
                angles++; // a
                var angle2 = Vector3.Angle(normal, edge2.normal);
                anglesSum += angle2;
                //if (angle2 < 0 || angle2 > 45) return false;
            }

            var angle = Vector3.Angle(normal, e_ij.normal);
            //if (angle < 0 || angle > 45) return false;

            anglesSum += angle;

            if (angles == 3 && anglesSum > 180f) return false;
            else if (angles == 2 && anglesSum > 140f) return false;
            else if (angles == 1 && anglesSum > 90f) return false;
        }
        
        Triangle candidate = new Triangle(PointCloud[e_ij.vertex1].Position, PointCloud[e_ij.vertex2].Position, PointCloud[m].Position, new int[] { e_ij.vertex1, e_ij.vertex2, m });

        foreach (var triangle in triangles)
        {
            if (!triangle.GeomIntegrity(candidate, allowedPoints, fixedEdges, fixedVertices))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Calculate value of the weighted energy function for existing triangle and 
    /// a triangle that is a candidate.
    /// This function is used for choosing the most appropriate triangle.
    /// </summary>
    /// <param name="i">First vertex of an active edge</param>
    /// <param name="j">Second vertex of the active edge</param>
    /// <param name="k">Third vertex of the existing triangle</param>
    /// <param name="m">Candidate</param>
    /// <returns>The value</returns>
    private float CalculateEnergy(int i, int j, int k, int m)
    {
        if (energyFunction2) return CalculateEnergy2(i, j, m);
        float L_ij = SqrDistance(i, j);
        float L_im = SqrDistance(i, m);
        float L_jm = SqrDistance(j, m);
       // if (L_im > 1000f || L_jm > 1000f) return float.PositiveInfinity;
        float L_ik = SqrDistance(i, k);
        float L_jk = SqrDistance(j, k);
        float A_ijm = FindTriangleArea(i, j, m);
        float A_ijk = FindTriangleArea(i, j, k);

        float k_ij = (L_ik + L_jk - L_ij) / A_ijk + (L_im + L_jm - L_ij) / A_ijm;
        float k_im = 2 * (L_im + L_jm - L_ij) / A_ijm;
        float k_jm = k_im;

        return k_ij * L_ij + k_im * L_im + k_jm * L_jm;
    }

    private float CalculateEnergy2(int i, int j, int m)
    {
        float L_ij = SqrDistance(i, j);
        float L_im = SqrDistance(i, m);
        float L_jm = SqrDistance(j, m);
        return (L_ij + L_im + L_jm) * (L_im + L_jm - L_ij) / FindTriangleArea(i, j, m);
    }

    /// <summary>
    /// Calculate triangle area.
    /// </summary>
    /// <param name="i">First vertex</param>
    /// <param name="j">Second vertex</param>
    /// <param name="k">Thirs vertex</param>
    /// <returns>The area of the triangle.</returns>
    private float FindTriangleArea(int i, int j, int k)
    {
        float a = Distance(i, j);
        float b = Distance(i, k);
        float c = Distance(j, k);

        float s = (a + b + c) / 2;
        return Mathf.Sqrt(s * (s - a) * (s - b) * (s - c));
    }
    
    private float SqrDistance(int a, int b)
    {
        return Vector3.SqrMagnitude(PointCloud[a].Position - PointCloud[b].Position);
    }

    private float Distance(int a, int b)
    {
        return Vector3.Distance(PointCloud[a].Position, PointCloud[b].Position);
    }
    
    private void AddTriangle(Edge edge, int point, Queue<Edge> aeq, Dictionary<Edge, Edge> edges, HashSet<Edge> fixedEdges, List<Triangle> triangles, HashSet<int> fixedVertices)
    {
        Triangle triangle = new Triangle(PointCloud[point].Position, PointCloud[edge.vertex1].Position, PointCloud[edge.vertex2].Position, new int[] { point, edge.vertex1, edge.vertex2 });
        PointCloud[point].triangles.Add(triangle);
        PointCloud[edge.vertex1].triangles.Add(triangle);
        PointCloud[edge.vertex2].triangles.Add(triangle);

        Vector3 normal = Vector3.Cross(PointCloud[edge.vertex1].Position - PointCloud[point].Position, PointCloud[edge.vertex2].Position - PointCloud[point].Position).normalized;
        meshTriangles.Add(point);
        if (!newNormalByDot)
        {
            int i = edge.vertex1;
            int j = edge.vertex2;
            int k = edge.vertex3;

            // check if the i and j vertices are chosen correctly
            if (Vector3.Dot(Vector3.Cross(PointCloud[k].Position - PointCloud[i].Position, PointCloud[k].Position - PointCloud[j].Position), edge.normal) < 0)
            {
                i = edge.vertex2;
                j = edge.vertex1;
            }

            normal = Vector3.Cross(PointCloud[point].Position - PointCloud[j].Position, PointCloud[point].Position - PointCloud[i].Position).normalized;

            meshTriangles.Add(j);
            meshTriangles.Add(i);
        }
        else
        {
            float dot = Vector3.Dot(normal, edge.normal);
            if (dot <= 0)
            {
                meshTriangles.Add(edge.vertex2);
                meshTriangles.Add(edge.vertex1);
                normal *= -1;
            }
            else
            {
                meshTriangles.Add(edge.vertex1);
                meshTriangles.Add(edge.vertex2);
            }
        }
        

        Edge edge1 = new Edge(edge.vertex1, point, edge.vertex2, normal, Distance(edge.vertex1, point));
        Edge edge2 = new Edge(edge.vertex2, point, edge.vertex1, normal, Distance(edge.vertex2, point));

        fixedEdges.Add(edge);

        if (!edges.ContainsKey(edge1))
        {
            PointCloud[edge.vertex1].AddEdge(edge1);
            PointCloud[point].AddEdge(edge1);
            aeq.Enqueue(edge1);
            edges.Add(edge1, edge1);
        } else
        {
            PointCloud[edge.vertex1].RemoveActiveEdge();
            PointCloud[point].RemoveActiveEdge();
            fixedEdges.Add(edge1);
        }

        if (!edges.ContainsKey(edge2))
        {
            PointCloud[edge.vertex2].AddEdge(edge2);
            PointCloud[point].AddEdge(edge2);
            aeq.Enqueue(edge2);
            edges.Add(edge2, edge2);
        }
        else
        {
            PointCloud[edge.vertex2].RemoveActiveEdge();
            PointCloud[point].RemoveActiveEdge();
            fixedEdges.Add(edge2);
        }
        PointCloud[edge.vertex1].RemoveActiveEdge();
        PointCloud[edge.vertex2].RemoveActiveEdge();
        triangles.Add(triangle);

        if (PointCloud[edge.vertex1].status == PointStatus.FIXED) fixedVertices.Add(edge.vertex1);
        if (PointCloud[edge.vertex2].status == PointStatus.FIXED) fixedVertices.Add(edge.vertex2);
        if (PointCloud[point].status == PointStatus.FIXED) fixedVertices.Add(point);
    }

    /// <summary>
    /// Create seed triangle 0.
    /// </summary>
    /// <param name="aeq">Active-edge queue</param>
    /// <param name="edges">Hashset with all edges</param>
    private void GenerateSeedTriangle(Queue<Edge> aeq, Dictionary<Edge, Edge> edges, List<Triangle> triangles)
    {
        int[] seedTriangle = GetSeedTriangle();
        AddSeedTriangle(seedTriangle);
    }

    private void AddSeedTriangle(int[] seedTriangle)
    {
        int corner1 = seedTriangle[0], corner2 = seedTriangle[1], corner3 = seedTriangle[2];
        Vector3 normal = Vector3.Cross(PointCloud[corner2].Position - PointCloud[corner1].Position, PointCloud[corner3].Position - PointCloud[corner1].Position).normalized;
        //Vector3 direction = points[corner1].CameraDirection + points[corner2].CameraDirection + points[corner3].CameraDirection;

        float dot = Vector3.Dot(normal, new Vector3(0, 0, -1));

        meshTriangles.Add(corner1);
        if (dot <= 0)
        {
            meshTriangles.Add(corner3);
            meshTriangles.Add(corner2);
            normal *= -1;
        }
        else
        {
            meshTriangles.Add(corner2);
            meshTriangles.Add(corner3);
        }

        Edge edge1 = new Edge(corner2, corner3, corner1, normal, Distance(corner2, corner3));
        Edge edge2 = new Edge(corner1, corner3, corner2, normal, Distance(corner1, corner3));
        Edge edge3 = new Edge(corner2, corner1, corner3, normal, Distance(corner2, corner1));

        PointCloud[corner1].AddEdge(edge2);
        PointCloud[corner1].AddEdge(edge3);

        PointCloud[corner2].AddEdge(edge1);
        PointCloud[corner2].AddEdge(edge3);

        PointCloud[corner3].AddEdge(edge1);
        PointCloud[corner3].AddEdge(edge2);

        Triangle triangle = new Triangle(PointCloud[corner1].Position, PointCloud[corner2].Position, PointCloud[corner3].Position, new int[] { corner1, corner2, corner3 });
        PointCloud[corner1].triangles.Add(triangle);
        PointCloud[corner2].triangles.Add(triangle);
        PointCloud[corner3].triangles.Add(triangle);

        aeq.Enqueue(edge1);
        aeq.Enqueue(edge2);
        aeq.Enqueue(edge3);

        edges.Add(edge1, edge1);
        edges.Add(edge2, edge2);
        edges.Add(edge3, edge3);

        triangles.Add(triangle);
    }
    private int[] GetSeedTriangle()
    {
        int[] vertices = GetFirstTwoPoints();
        vertices[2] = GetThirdInitialPoint(vertices[0], vertices[1]);

        return vertices;
    }

    /// <summary>
    /// Get first two points of the seed triangle.
    /// </summary>
    /// <returns>Array of indexes of first two points.</returns>
    private int[] GetFirstTwoPoints()
    {
        int vertex1 = 42;
        int vertex2 = 42;

        float maxZ = float.NegativeInfinity; // set to max y!
        for (int i = 0; i < PointCloud.Count; ++i)
        {
            if (PointCloud[i].Position.z > maxZ)
            {
                maxZ = PointCloud[i].Position.z;
                vertex1 = i;
            }
        }

        float minDist = float.PositiveInfinity;
        for (int i = 0; i < PointCloud.Count; ++i)
        {
            float dist = Vector3.Distance(PointCloud[i].Position, PointCloud[vertex1].Position);
            if (dist < minDist && i != vertex1)
            {
                minDist = dist;
                vertex2 = i;
            }
        }

        return new int[3] {vertex1, vertex2, -1};
    }

    /// <summary>
    /// Get the third vertex of the seed triangle.
    /// This is adopted algorithm from the original paper.
    /// </summary>
    /// <param name="vertex1">First vertex</param>
    /// <param name="vertex2">Second vertex</param>
    /// <returns></returns>
    private int GetThirdInitialPoint(int vertex1, int vertex2)
    {
        int[] candidates = GetThirdInitialPointCandidates(vertex1, vertex2);
        Vector3 _vertex1 = PointCloud[vertex1].Position;
        Vector3 _vertex2 = PointCloud[vertex2].Position;

        int current = -1;
        float minDistance = float.PositiveInfinity;

        foreach (var candidate in candidates)
        {
            Vector3 _vertex3 = PointCloud[candidate].Position;
            float currentDistance = Vector3.Distance(_vertex1, _vertex3) + Vector3.Distance(_vertex2, _vertex3);

            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                current = candidate;
            }
        }

        return current;
    }

    /// <summary>
    /// Get potential points that are used as the third vertex for seed triangle.
    /// </summary>
    /// <returns>Array of indexes of potential points</returns>
    private int[] GetThirdInitialPointCandidates(int vertex1, int vertex2)
    {
        Vector3 axisDirection = PointCloud[vertex2].Position - PointCloud[vertex1].Position;
        Vector3 middlePoint = (PointCloud[vertex1].Position + PointCloud[vertex2].Position) / 2;
        float shortestDistance = float.PositiveInfinity;
        List<int> currentCandidates = new List<int>();

        for (int i = 0; i < PointCloud.Count; ++i)
        {
            if (i == vertex1 || i == vertex2) continue;

            Vector3 current = PointCloud[i].Position;
            float distanceToAxisSqr = 
                Vector3.SqrMagnitude(Vector3.Cross(PointCloud[vertex1].Position - current, axisDirection)) /
                Vector3.SqrMagnitude(axisDirection);

            float currentDistanceSqr = Math.Max(distanceToAxisSqr, Vector3.SqrMagnitude(middlePoint - current));

            if (currentDistanceSqr < shortestDistance)
            {
                shortestDistance = currentDistanceSqr;
                currentCandidates.Clear();
            }

            if (currentDistanceSqr == shortestDistance)
                currentCandidates.Add(i);
        }
        return currentCandidates.ToArray();
    }
}