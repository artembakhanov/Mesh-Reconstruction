using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(PointStorage))]
public class PointCloudVisualizer : MonoBehaviour
{
    public bool ShowParticles = true;
    public float ParticleSize = 0.02f;
    public Color ParticleColor = new Color(0, 255, 32);
    private ParticleSystem particleSystem;
    private PointStorage pointStorage;

    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        pointStorage = GetComponent<PointStorage>();

        pointStorage.voxelSet.UpdateEvent += VoxelSet_UpdateEvent;
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
            if (pointStorage.voxelSet.version - points[i].Version == 1)
                particles[i].remainingLifetime = 1e5f - 0.01f * i;
            else
                particles[i].remainingLifetime = 1e5f - 5f;
        }
        particleSystem.SetParticles(particles);


        //ParticleSystem.Particle[] particles = new ParticleSystem.Particle[points.Count];
        //for (int i = 0; i < points.Count; ++i) {
        //    //particles[i] = new ParticleSystem.Particle
        //    //{
        //    //    startColor = ParticleColor,
        //    //    startSize = ParticleSize,
        //    //    position = points[i].Position,
        //    //    remainingLifetime = 10000f
        //    //};
        //}
        //particleSystem.SetParticles(particles);
    }
}
