using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(PointStorage))]
public class PointCloudVisualizer : MonoBehaviour
{
    public bool ShowParticles = true;
    public float ParticleSize = 0.005f;
    public Color ParticleColor = new Color(0, 255, 225);
    private ParticleSystem particleSystem;
    private PointStorage pointStorage;

    // Start is called before the first frame update
    void Start()
    {
        pointStorage = GetComponent<PointStorage>();
        pointStorage.voxelSet.UpdateEvent += VoxelSet_UpdateEvent;

        particleSystem = GetComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.startSize = ParticleSize;
        main.startColor = ParticleColor;

     }

    private void VoxelSet_UpdateEvent()
    {
        if (!ShowParticles) return;
        var points = pointStorage.voxelSet.Points;

        particleSystem.SetParticles(new ParticleSystem.Particle[0]);
        int count = points.Count;
        particleSystem.Emit(count);
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[count];
        particleSystem.GetParticles(particles);
        for (int i = 0; i < count; i++)
        {
            //particles[i].color = gradient.Evaluate(Vector3.Distance(camPos, positions[i]) * d);
            particles[i].position = points[i].Position;
            //if (pointStorage.voxelSet.version - points[i].Version == 1)
            //    particles[i].remainingLifetime = 100000f - 0.01f * i;
            //else
            //{
            //    //particles[i].remainingLifetime = 100000f - 5f;
            //    particles[i].startColor = new Color(255, 0, 0);
            //}
        }
        particleSystem.SetParticles(particles);
    }
}
