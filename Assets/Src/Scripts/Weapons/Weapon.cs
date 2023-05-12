using System.Collections.Generic;
using System.Linq;
using Src.Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Weapons
{
    public class Weapon : MonoBehaviour
    {
        public float ammoRemaining;
        public float maxAmmo;
        public float initialUsage; // How much ammo to consume when starting to fire
        public float refillRate;
        public float usageRate;
        public float lowAmmoThreshold; // Ammo indicator is shown when ammo reaches this threshold
        public bool hideUIAboveThreshold = true;
        public AudioClip ammoFullSFX;
        public AudioClip ammoRefillSFX;
        public AmmoUI ammoUI;
        public GameObject UIObject;
        public Animator weaponAnimator;
        public float AmmoNormalized => ammoRemaining / maxAmmo;
        public List<Renderer> renderers;
        public AudioSource audioSource;

        
        private XRGrabInteractable _interactable;
        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>().ToList();
            _interactable = GetComponent<XRGrabInteractable>();
        }

        protected virtual bool ConsumeAmmo(float amount)
        {
            if (ammoRemaining >= amount)
            {
                ammoRemaining -= amount;
                UpdateAmmoUI();
                return true;
            }

            return false;
        }
        
        public virtual bool ConsumeAmmo()
        {
            if (ConsumeAmmo(Time.deltaTime * usageRate))
            {
                return true;
            }
            return false;
        }

        public virtual void RefillAmmo(float amount)
        {
            ammoRemaining += amount;
            ammoRemaining = Mathf.Clamp(ammoRemaining, 0, maxAmmo);
            if (hideUIAboveThreshold && ammoRemaining >= lowAmmoThreshold)
            {
                HideUI();
            }

            if (ammoRemaining >= maxAmmo)
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
            if (ammoRemaining >= maxAmmo)
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
            if (ammoRemaining >= maxAmmo) return;
            RefillAmmo(Time.deltaTime * refillRate);
            UpdateAmmoUI();
        }

        private void UpdateAmmoUI()
        {
            if (ammoUI != null)
            {
                ammoUI.SetAmount(AmmoNormalized);
                if (ammoRemaining <= lowAmmoThreshold)
                {
                    ShowUI();
                }
            }
        }

        public void HideWeapon()
        {
            foreach (var rend in renderers)
            {
                rend.enabled = false;
            }

            _interactable.activatable = false;
        }

        public void ShowWeapon()
        {
            foreach (var rend in renderers)
            {
                rend.enabled = true;
            }
            _interactable.activatable = true;
        }

        public void HideUI()
        {
            UIObject.SetActive(false);
        }

        public void ShowUI()
        {
            UIObject.SetActive(true);
        }

        public void SetUIColor(Color color)
        {
            if (ammoUI != null)
            {
                ammoUI.SetColor(color);
            }
        }
    }
}
