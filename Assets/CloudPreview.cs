using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudPreview : MonoBehaviour
{
    private ParticleSystem cloudSystem;
    // Start is called before the first frame update
    void Start()
    {
        cloudSystem = GetComponent<ParticleSystem>();
        cloudSystem.SetParticles(Consts.particles.ToArray());
    }

    void FixedUpdate()
    {
        cloudSystem.transform.Rotate(new Vector3(0, 1, 0));
    }
}
