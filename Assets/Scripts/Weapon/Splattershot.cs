using System;
using UnityEngine;

namespace Weapon
{
    public class Splattershot : Weapon
    {
        public ParticleSystem mainParticle;

        private bool firing;

        // Update is called once per frame
        void Update()
        {
            if (firing && mainParticle.isPlaying)
            {
                if (!ConsumeAmmo())
                {
                    EndFire(); // Out of ammo, stop shooting
                }
            }
        }

        public void StartFire()
        {
            if (ConsumeAmmo(initialUsage))
            {
                mainParticle.Play();
                firing = true;
            }
        }

        public void EndFire()
        {
            mainParticle.Stop();
            firing = false;
        }
    }
}
