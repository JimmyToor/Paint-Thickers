using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSFXController : MonoBehaviour
{
    public SFXSource sfxSource;
    
    private ParticleSystem partSystem;
    private int particleCount;
    
    // Start is called before the first frame update
    void Start()
    {
        partSystem = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        int newParticleCount = partSystem.particleCount;
        if (newParticleCount > particleCount)
        {
            sfxSource.TriggerPlay(transform.position);
        }
        particleCount = newParticleCount;
    }
}
