using UnityEngine;

namespace Src.Scripts.Audio
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticleSFXController : MonoBehaviour
    {
        public SFXSource sfxSource;
    
        private ParticleSystem _partSystem;
        private int _particleCount;
    
        // Start is called before the first frame update
        void Start()
        {
            _partSystem = GetComponent<ParticleSystem>();
        }

        // Update is called once per frame
        void Update()
        {
            int newParticleCount = _partSystem.particleCount;
            if (newParticleCount > _particleCount)
            {
                sfxSource.TriggerPlay(transform.position);
            }
            _particleCount = newParticleCount;
        }
    }
}
