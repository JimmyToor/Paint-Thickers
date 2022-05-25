using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSFXController : MonoBehaviour
{
    public SFXSource sfxSource;
    
    private ParticleSystem particleSystem;
    private int particleCount;
    
    // Start is called before the first frame update
    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        int newParticleCount = particleSystem.particleCount;
        if (newParticleCount > particleCount)
        {
            sfxSource.TriggerPlay(transform.position);
        }
        particleCount = newParticleCount;
    }
}
