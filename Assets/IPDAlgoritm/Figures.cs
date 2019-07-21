using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Bakhanov.IPD
{
    public class Point
    {
        public readonly Vector3 Position;
        public float UniformityDegree => MinEdge == 0 ? 0 : MaxEdge / MinEdge;
        public float MaxEdge { get; private set; } = 0;
        public float MinEdge { get; private set; } = 10000f;
        public PointStatus status = PointStatus.ACTIVE;
        public List<Triangle> triangles = new List<Triangle>();
        private int activeEdges = 0;

        public Point(Vector3 position)
        {
            Position = position;
        }

        /// <summary>
        /// Add new incident edge
        /// Update 
        /// </summary>
        /// <param name="edge">The size of new edge</param>
        public void AddEdge(Edge edge)
        {
            if (edge.length > MaxEdge) MaxEdge = edge.length;
            if (edge.length < MinEdge) MinEdge = edge.length;

            activeEdges++;
            if (activeEdges > 0) status = PointStatus.ACTIVE;
        }

        public void RemoveActiveEdge()
        {
            activeEdges--;
            if (activeEdges == 0) status = PointStatus.FIXED;
        }
    }

    public class Edge { 
        public readonly int vertex1;
        public readonly int vertex2;
        public readonly int vertex3; // used for storing vertex of a triangle 
        public readonly Vector3 normal;
        public readonly float length;
        public EdgeStatus status = EdgeStatus.ACTIVE;

        public Edge(int vertex1, int vertex2)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
        }

        public Edge(int vertex1, int vertex2, Vector3 normal)
        {
            this.vertex1 = vertex1;
            this.vertex2 = vertex2;
            this.normal = normal;
        }

        public Edge(int vertex1, int vertex2, Vector3 normal, float length) : this(vertex1, vertex2, normal)
        {
            this.length = length;
        }

        public Edge(int vertex1, int vertex2, int vertex3, Vector3 normal, float length) : this (vertex1, vertex2, normal, length)
        {
            this.vertex3 = vertex3;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Edge o = obj as Edge;

            if (obj == null) return false;
            else return (vertex1 == o.vertex1 && vertex2 == o.vertex2 || vertex1 == o.vertex2 && vertex2 == o.vertex1);
        }

        public override int GetHashCode()
        {
            return (vertex1.GetHashCode() * vertex2.GetHashCode()).GetHashCode();
        }
    }

    public class Triangle
    {
        public readonly Vector3[] vertices;
        public readonly int[] vertInds;
        public readonly Face face;
        public static int counter3d = 0;
        public static int counter2d = 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="v1">Position of the first vertex</param>
        /// <param name="v2">Position of the second vertex</param>
        /// <param name="v3">Position of the third vertex</param>
        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            vertices = new Vector3[] { v1, v2, v3 };
            face = new Face(Vector3.Cross(v2 - v1, v3 - v1).normalized, v1);
        }

        /// <summary>
        /// Constructor with vertex index
        /// </summary>
        /// <param name="v1">Position of the first vertex</param>
        /// <param name="v2">Position of the second vertex</param>
        /// <param name="v3">Position of the third vertex</param>
        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3, int[] vertInds) : this(v1, v2, v3)
        {
            this.vertInds = vertInds;
        }

        /// <summary>
        /// Check geometry integrity for two triangles (this and o)
        /// </summary>
        /// <param name="o">Another triangle</param>
        /// <param name="allowedPoints">Points, which the triangle is allowed to intersect with</param>
        /// <param name="fixedEdges">Fixed edges</param>
        /// <returns></returns>
        public bool GeomIntegrity(Triangle o, Vector3[] allowedPoints, HashSet<Edge> fixedEdges, HashSet<int> fixedVertices)
        {
            bool geomIntegrity = false;

            bool antiparallel = false;
            if (this.face.normal == o.face.normal || (antiparallel = this.face.normal == -o.face.normal))
            {
                // planes are parallel
                float D = (antiparallel ? -1 : 1) * o.face.D;
                if (D - 1e-6f <= this.face.D && this.face.D <= D + 1e-6f)
                    geomIntegrity = Check2d(o, allowedPoints);
                else
                    geomIntegrity = true;

            } else
            {
                // planes are not parallel  
                geomIntegrity = Сheck3d(o, allowedPoints, fixedEdges, fixedVertices); 
            }

            return geomIntegrity;
        }

        /// <summary>
        /// Check geometry integrity in 3d case
        /// </summary>
        /// <param name="o">Another triangle</param>
        /// <param name="allowedPoints">Points, which the triangle is allowed to intersect with</param>
        /// <param name="fixedEdges">Fixed edges</param>
        /// <returns></returns>
        private bool Сheck3d(Triangle o, Vector3[] allowedPoints, HashSet<Edge> fixedEdges, HashSet<int> fixedPoints)
        {
            ++counter3d;
            int comp = 42;
            float[] pt1 = new float[3];
            float[] pt2 = new float[3];

            int intersect = TriangleOverlapTest.Test(this, o, ref comp, pt1, pt2);
            if (intersect == 0) return true;

            Vector3 point1 = new Vector3(pt1[0], pt1[1], pt1[2]);
            Vector3 point2 = new Vector3(pt2[0], pt2[1], pt2[2]);

            int common1_1 = -1, common2_1 = -1;
            FindCommonPoints(out common1_1, this, point1);
            FindCommonPoints(out common2_1, o, point1);
            if ((point1 - point2).magnitude < 1e-6f)
            {
                if (common1_1 != common2_1) return false;
                if (fixedPoints.Contains(common1_1) || fixedPoints.Contains(common2_1)) return false;
                foreach (var ap in allowedPoints)
                {
                    if ((ap - point1).magnitude < 1e-6f)
                        return true;
                }
                return false;//common1_1 == 1 && common2_1 == 1;
            }
            else
            {
                int common1_2, common2_2;
                FindCommonPoints(out common1_2, this, point2);
                FindCommonPoints(out common2_2, o, point2);

                if(common1_1 == -1 || common1_2 == -1 || common2_1 == -1 || common2_2 == -1) return false;
                return !fixedEdges.Contains(new Edge(common1_1, common1_2)) && !fixedEdges.Contains(new Edge(common2_1, common2_2));
            }
        }

        /// <summary>
        /// This is used if o lies in the plane of this triangle
        /// </summary>
        /// <param name="o">Another triangle</param>
        /// <returns>True if geometry integrity holds</returns>
        private bool Check2d(Triangle o, Vector3[] allowedPoints)
        {
            counter2d++;
            List<Vector3> vers = new List<Vector3> { this.vertices[0], this.vertices[1], this.vertices[2] };
            List<Vector3> vers2 = new List<Vector3> { o.vertices[0], o.vertices[1], o.vertices[2] };
            List<Vector3> commonVers = new List<Vector3>();
            foreach (var ver in o.vertices)
            {
                if (vers.Contains(ver))
                {
                    vers.Remove(ver);
                    vers2.Remove(ver);
                    commonVers.Add(ver);
                }
            }

            Vector2 pi1 = new Vector2(0, 0);
            Vector2 pi2 = new Vector2(0, 0);
            foreach (var ver in o.vertices)
            {
                pi1 += IsPointInside(this, ver);
            }

            foreach (var ver in vertices)
            {
                pi2 += IsPointInside(o, ver);
            }

            if (commonVers.Count == 3)
                return false;
            if (commonVers.Count == 1)
            {
                foreach (var ap in allowedPoints)
                {
                    if ((ap - commonVers[0]).magnitude < 1e-6f)
                        return true;
                }

                return false;
            }
            if (commonVers.Count == 2)
            {
                Vector3 axis = commonVers[1] - commonVers[0];
                return Vector3.Dot(Vector3.Cross(axis, vers[0] - commonVers[0]), Vector3.Cross(axis, vers2[0] - commonVers[0])) < 0
                    && pi1.Equals(new Vector2(0, commonVers.Count)) && pi2.Equals(new Vector2(0, commonVers.Count));
            }
            else
                return pi1.Equals(new Vector2(0, commonVers.Count)) && pi2.Equals(new Vector2(0, commonVers.Count));
        }

        private void FindCommonPoints(out int common, Triangle t, Vector3 point1)
        {
            common = -1;

            for (int i = 0; i < 3; ++i)
            {
                if ((t.vertices[i] - point1).magnitude < 1e-6f) common = t.vertInds[i];
                
            }
        }

        private Vector2 IsPointInside(Triangle t, Vector3 p)
        {
            Vector3 a = t.vertices[0];
            Vector3 b = t.vertices[1];
            Vector3 c = t.vertices[2];
            Vector3 ab = b - a;
            Vector3 ap = p - a;
            Vector3 bc = c - b;
            Vector3 bp = p - b;
            Vector3 ca = a - c;
            Vector3 cp = p - c;

            Vector3 ab_ap = Vector3.Cross(ab, ap);
            Vector3 bc_bp = Vector3.Cross(bc, bp);
            Vector3 ca_cp = Vector3.Cross(ca, cp);

            bool ab_ap_zero = ab_ap.sqrMagnitude > -1e-6f && ab_ap.sqrMagnitude < 1e-6f;
            bool bc_bp_zero = bc_bp.sqrMagnitude > -1e-6f && bc_bp.sqrMagnitude < 1e-6f;
            bool ca_cp_zero = ca_cp.sqrMagnitude > -1e-6f && ca_cp.sqrMagnitude < 1e-6f;

            bool onSegment = true;
            if (ab_ap_zero && !OnSegment(a, b, p))
                onSegment = false;
            if (bc_bp_zero && !OnSegment(b, c, p))
                onSegment = false;
            if (ca_cp_zero && !OnSegment(c, a, p))
                onSegment = false;

            if (ab_ap_zero || bc_bp_zero || ca_cp_zero) {
                if (onSegment)
                    return new Vector2(0, 1);
                else
                    return new Vector2(0, 0);
            }
            else if (Vector3.Dot(ab_ap, bc_bp) > 0 && Vector3.Dot(ab_ap, ca_cp) > 0) return new Vector2(1, 0);
            else return new Vector2(0, 0);
        }

        private bool OnSegment(Vector3 a, Vector3 b, Vector3 p)
        {
            float x1 = Math.Min(a.x, b.x), x2 = Math.Max(a.x, b.x);
            float y1 = Math.Min(a.y, b.y), y2 = Math.Max(a.y, b.y);
            float z1 = Math.Min(a.z, b.z), z2 = Math.Max(a.z, b.z);

            return x1 <= p.x && p.x <= x2 && y1 <= p.y && p.y <= y2 && z1 <= p.z && p.z <= z2;
        }

        public override int GetHashCode()
        {
            return vertices[0].GetHashCode() + vertices[1].GetHashCode() + vertices[2].GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Triangle o = obj as Triangle;

            HashSet<Vector3> v = new HashSet<Vector3>();
            for (int i = 0; i < 3; ++i)
            {
                v.Add(o.vertices[i]);
            }

            for (int i = 0; i < 3; ++i)
            {
                if (!v.Contains(vertices[i]))
                    return false;
            }

            return true;
        }
    }
 }