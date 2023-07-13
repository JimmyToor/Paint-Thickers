using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Weapons
{
    [Serializable]
    public struct WeaponParameters
    {
        public float maxAmmo;
        [Tooltip("How much ammo to consume when starting to fire.")]
        public float initialUsage;
        public float refillRate;
        public float usageRate;
        [Tooltip("Ammo refills automatically once below this threshold.")]
        public float lowRefillThreshold;
        [Tooltip("Seconds to wait after falling below ammo threshold to refill ammo.")]
        public float lowAmmoRegenCooldownTime;
    }
    
    public class Weapon : MonoBehaviour
    {
        public WeaponParameters wepParams;
        [HideInInspector]
        public float ammoNormalized;
        
        [Header("SFX")]
        public AudioSource audioSource;
        public AudioClip ammoFullSFX;
        public AudioClip ammoRefillSFX;
        [Header("Animation")]
        public Animator weaponAnimator;
        
        public Action<float> onAmmoNormalizedChanged;
        public Action<Color> onColorChanged;
        public Action onWeaponEquipped;
        public Action onWeaponUnequipped;
        public Action onWeaponHidden;
        public Action onWeaponUnhidden;
        
        public List<Renderer> Renderers { get; set; }
        
        [SerializeField]
        private float ammoRemaining;
        public float AmmoRemaining
        {
            get => ammoRemaining;
            set
            {
                ammoRemaining = value;
                ammoNormalized = ammoRemaining / wepParams.maxAmmo;
                onAmmoNormalizedChanged?.Invoke(ammoNormalized);
            }
        }
        
        private XRGrabInteractable _interactable;
        private float _lowAmmoRegenCooldown;
        
        private void Awake()
        {
            Renderers = GetComponentsInChildren<Renderer>().ToList();
            _interactable = GetComponent<XRGrabInteractable>();
            _lowAmmoRegenCooldown = wepParams.lowAmmoRegenCooldownTime;
            wepParams.maxAmmo = Mathf.Max(1, wepParams.maxAmmo);
            AmmoRemaining = wepParams.maxAmmo;
        }

        protected virtual void FixedUpdate()
        {
            if (_lowAmmoRegenCooldown <= 0 && wepParams.lowRefillThreshold >= AmmoRemaining)
            {
                RefillAmmo();
            }
            else
            {
                _lowAmmoRegenCooldown -= Time.deltaTime;
            }
        }

        protected virtual bool ConsumeAmmo(float amount)
        {
            if (AmmoRemaining >= amount)
            {
                AmmoRemaining -= amount;
                _lowAmmoRegenCooldown = wepParams.lowAmmoRegenCooldownTime;
                return true;
            }

            return false;
        }
        
        public virtual bool ConsumeAmmo()
        {
            if (ConsumeAmmo(Time.deltaTime * wepParams.usageRate))
            {
                return true;
            }
            return false;
        }

        public virtual void RefillAmmo(float amount)
        {
            AmmoRemaining += amount;
            AmmoRemaining = Mathf.Clamp(AmmoRemaining, 0, wepParams.maxAmmo);

            if (AmmoRemaining >= wepParams.maxAmmo)
            {
                PlayFullSfx();
            }
            else
            {
                PlayReloadSfx();
            }
        }

        public void PlayFullSfx()
        {
            if (AmmoRemaining >= wepParams.maxAmmo)
            {
                StopReloadSfx();
                audioSource.PlayOneShot(ammoFullSFX);
            }
        }

        public void PlayReloadSfx()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = ammoRefillSFX;
                audioSource.Play();
            }
        }

        public void StopReloadSfx()
        {
            audioSource.Stop();
        }

        public virtual void RefillAmmo()
        {
            if (AmmoRemaining >= wepParams.maxAmmo) return;
            RefillAmmo(Time.deltaTime * wepParams.refillRate);
        }
        
        public void HideWeapon()
        {
            foreach (var rend in Renderers)
            {
                rend.enabled = false;
            }

            _interactable.activatable = false;
            onWeaponHidden?.Invoke();
        }

        public void ShowWeapon()
        {
            foreach (var rend in Renderers)
            {
                rend.enabled = true;
            }
            _interactable.activatable = true;
            onWeaponUnhidden?.Invoke();
        }
        
        public void SetColor(Color color)
        {
            onColorChanged?.Invoke(color);
        }

        public void DisableColliders()
        {
            _interactable.DisableColliders();
        }
        
        public void EnableColliders()
        {
            _interactable.EnableColliders();
        }
    }
}
