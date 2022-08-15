using System;
using UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Weapons
{
    public class Weapon : MonoBehaviour
    {
        public float ammoRemaining;
        public float maxAmmo;
        public float initialUsage; // How much ammo to initially consume on fire
        public float refillRate;
        public float usageRate;
        public float lowAmmoThreshold; // Ammo indicator is shown when ammo reaches this threshold
        public bool hideUIAboveThreshold = true;
        public AmmoUI ammoUI;
        public GameObject UIObject;
        public GameObject weaponObject;
        public Animator weaponAnimator;
        
        public float AmmoNormalized => ammoRemaining / maxAmmo;
        
        public virtual bool ConsumeAmmo(float amount)
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
        }

        public virtual void RefillAmmo()
        {
            RefillAmmo(Time.deltaTime * refillRate);
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
            weaponObject.SetActive(false);
        }

        public void ShowWeapon()
        {
            weaponObject.SetActive(true);
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
