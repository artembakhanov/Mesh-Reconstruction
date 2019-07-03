using System.Collections.Generic;
using UnityEngine;

public static class Consts
{
    public static List<ParticleSystem.Particle> particles;
    public static Color particlesColor = new Color(0, 255, 32);
    public static float voxelSize = 0.02f;
    public static bool checkRadix = true;
    public static float radiusCrust = 20f;
    public static bool drawVoxels = false;
    public static bool drawMeshIT = false;
    public static float crustPositionCounst = 100f;
    public static float ballForce = 200f;
}