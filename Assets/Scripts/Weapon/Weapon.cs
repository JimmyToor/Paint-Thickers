using System;
using UnityEngine;

namespace Weapon
{
    public class Weapon : MonoBehaviour
    {
        public float ammoRemaining;
        public float maxAmmo;
        public float initialUsage; // How much ammo to initially consume on fire
        public float refillRate;
        public float usageRate;
        public float lowAmmoThreshold; // Ammo indicator is shown when ammo reaches this threshold
        public AmmoUI ammoUI;
        public GameObject UIObject;
        public GameObject weaponObject;

        public float AmmoNormalized => ammoRemaining / maxAmmo;
        
        private void LateUpdate()
        {
            if (ammoUI != null)
            {
                UpdateAmmoUI();
                if (ammoRemaining <= lowAmmoThreshold)
                {
                    ShowUI();
                }
            }
        }

        // Try and consume the specified amount of ammo
        // Return true if successful
        public virtual bool ConsumeAmmo(float amount)
        {
            if (ammoRemaining >= amount)
            {
                ammoRemaining -= amount;
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
        }

        public virtual void RefillAmmo()
        {
            RefillAmmo(Time.deltaTime * refillRate);
        }

        private void UpdateAmmoUI()
        {
            ammoUI.SetAmount(AmmoNormalized);
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
    }
}
