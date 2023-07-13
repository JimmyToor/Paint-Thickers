using UnityEngine;

namespace Src.Scripts.Weapons
{
    public class Splattershot : Weapon
    {
        public ParticleSystem mainParticle;
        
        public bool Firing { get; set; }
        
        private readonly int _firingBoolId = Animator.StringToHash("Firing");

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
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
            if (!ConsumeAmmo(wepParams.initialUsage)) return;
            
            mainParticle.Play();
            Firing = true;
            weaponAnimator.SetBool(_firingBoolId, true);
        }

        public void EndFire()
        {
            mainParticle.Stop();
            Firing = false;
            weaponAnimator.SetBool(_firingBoolId, false);
        }
    }
}
