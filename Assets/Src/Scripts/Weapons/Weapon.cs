using System;
using System.Collections.Generic;
using System.Linq;
using Src.Scripts.UI;
using UnityEngine;
using UnityEngine.Serialization;
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
        [Tooltip("Ammo indicator is shown when ammo reaches this threshold.")]
        public float lowAmmoThreshold;
        [Tooltip("Ammo refills automatically once below this threshold.")]
        public float lowRefillThreshold;
        public bool hideUIAboveThreshold;
    }
    
    public class Weapon : MonoBehaviour
    {
        public WeaponParameters wepParams;
        public float ammoRemaining;
        [Header("SFX")]
        public AudioSource audioSource;
        public AudioClip ammoFullSFX;
        public AudioClip ammoRefillSFX;
        [Header("UI")]
        public AmmoUI ammoUI;
        public GameObject uiObject;
        [Header("Animation")]
        public Animator weaponAnimator;
        
        public float AmmoNormalized => ammoRemaining / wepParams.maxAmmo;

        public List<Renderer> Renderers { get; set; }
        
        private XRGrabInteractable _interactable;

        private void Awake()
        {
            Renderers = GetComponentsInChildren<Renderer>().ToList();
            _interactable = GetComponent<XRGrabInteractable>();
        }

        private void Update()
        {
            if (wepParams.lowRefillThreshold > AmmoNormalized)
            {
                RefillAmmo();
            }
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
            if (ConsumeAmmo(Time.deltaTime * wepParams.usageRate))
            {
                return true;
            }
            return false;
        }

        public virtual void RefillAmmo(float amount)
        {
            ammoRemaining += amount;
            ammoRemaining = Mathf.Clamp(ammoRemaining, 0, wepParams.maxAmmo);
            if (wepParams.hideUIAboveThreshold && ammoRemaining >= wepParams.lowAmmoThreshold)
            {
                HideUI();
            }

            if (ammoRemaining >= wepParams.maxAmmo)
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
            if (ammoRemaining >= wepParams.maxAmmo)
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
            if (ammoRemaining >= wepParams.maxAmmo) return;
            RefillAmmo(Time.deltaTime * wepParams.refillRate);
            UpdateAmmoUI();
        }

        private void UpdateAmmoUI()
        {
            if (ammoUI != null)
            {
                ammoUI.SetAmount(AmmoNormalized);
                if (ammoRemaining <= wepParams.lowAmmoThreshold)
                {
                    ShowUI();
                }
            }
        }

        public void HideWeapon()
        {
            foreach (var rend in Renderers)
            {
                rend.enabled = false;
            }

            _interactable.activatable = false;
        }

        public void ShowWeapon()
        {
            foreach (var rend in Renderers)
            {
                rend.enabled = true;
            }
            _interactable.activatable = true;
        }

        public void HideUI()
        {
            uiObject.SetActive(false);
        }

        public void ShowUI()
        {
            uiObject.SetActive(true);
        }

        public void SetUIColor(Color color)
        {
            if (ammoUI != null)
            {
                ammoUI.SetColor(color);
            }
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
