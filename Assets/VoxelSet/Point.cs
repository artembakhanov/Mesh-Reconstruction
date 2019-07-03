using UnityEngine;
using UnityEditor;

namespace Bakhanov.VoxelSet{
    public class Point
    {
        public Color Color = Consts.particlesColor;
        public ulong Id;
        public Vector3 Position;
        public Vector3 RightHandedPosition;
        public Vector3 CameraDirection;
        public float ConfidenceValue;
        public float ColliderRadius = 0.01f;
        public int PointGroup;

        #region Constructors
        public Point(ulong id, Vector3 position, float confidenceValue)
        {
            ColliderRadius = VoxelSet.MaxColliderRadius;
            Id = id;
            Position = position;
            RightHandedPosition = new Vector3(position.x, position.z, position.y);
            ConfidenceValue = confidenceValue;
        }

        public Point(ulong id, Vector3 position, float confidenceValue, Vector3 cameraDirection) : this(id, position, confidenceValue)
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
        #endregion

        public Point Copy()
        {
            return new Point(Id, Position, ConfidenceValue, CameraDirection);
        }
    }
}