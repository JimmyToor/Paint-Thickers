using UnityEngine;

namespace Src.Scripts.Weapons
{
    public class Splattershot : Weapon
    {
        public ParticleSystem mainParticle;
        
        public bool Firing { get; set; }
        
        private int _firingBoolId = Animator.StringToHash("Firing");

        // Update is called once per frame
        void Update()
        {
            if (Firing && mainParticle.isPlaying)
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
                Firing = true;
                weaponAnimator.SetBool(_firingBoolId, true);
            }
        }

        public void EndFire()
        {
            mainParticle.Stop();
            Firing = false;
            weaponAnimator.SetBool(_firingBoolId, false);
        }
    }
}
