using UnityEngine;
using System.Collections;

namespace Bakhanov.VoxelSet
{
    public struct Triangle
    {
        public readonly int[] vertices;

        public Triangle(int vertex1, int vertex2, int vertex3)
        {
            vertices = new int[3];
            vertices[0] = vertex1;
            vertices[1] = vertex2;
            vertices[2] = vertex3;
        }
    }
}
