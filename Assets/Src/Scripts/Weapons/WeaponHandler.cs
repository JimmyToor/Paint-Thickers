using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Weapons
{
    public class WeaponHandler : MonoBehaviour
    {
        public Weapon Weapon { get; private set; }
        
        [SerializeField]
        private XRBaseInteractor leftInteractor;
        [SerializeField]
        private XRBaseInteractor rightInteractor;

        public void OnEnable()
        {
            leftInteractor.selectEntered.AddListener(TrySetWeapon);
            rightInteractor.selectExited.AddListener(TryRemoveWeapon);
        }
        public void OnDisable()
        {
            leftInteractor.selectEntered.RemoveListener(TrySetWeapon);
            rightInteractor.selectExited.RemoveListener(TryRemoveWeapon);
        }

        private void TrySetWeapon(SelectEnterEventArgs args)
        {
            if ((args.interactable).TryGetComponent(out Weapon weapon))
            {
                SetWeapon(weapon);
            }
        }
        private void TryRemoveWeapon(SelectExitEventArgs args)
        {
            if ((args.interactable).TryGetComponent(out Weapon _))
            {
                RemoveWeapon();
            }
        }

        public bool SetWeapon(Weapon newWeapon)
        {
            if (newWeapon == null)
            {
                Debug.LogError("{0} attempted to pickup a null weapon.", transform.gameObject);
                return false;
            }
            Weapon = newWeapon;
            return true;
        }

        public void ShowUI()
        {
            Weapon.ShowUI();
        }

        public void SetUIColor(Color color)
        {
            Weapon.SetUIColor(color);
        }
        
        public void RemoveWeapon()
        {
            Weapon = null;
        }

        public void DisableWeapon()
        {
            if (!Weapon) return;
            
            Weapon.gameObject.SetActive(false);
        }

        public void EnableWeapon()
        {
            if (!Weapon) return;
            
            Weapon.gameObject.SetActive(true);
        }

        public void HideWeapon()
        {
            if (!Weapon) return;

            Weapon.HideWeapon();
        }

        public void ShowWeapon()
        {
            if (!Weapon) return;
            Weapon.ShowWeapon();
        }

        public void EnableWeaponUI()
        {
            if (!Weapon) return;
            
            Weapon.ShowUI();
        }

        public void DisableWeaponUI()
        {
            if (!Weapon) return;
            
            Weapon.HideUI();
        }
    
        public void RefillWeaponAmmo()
        {
            if (!Weapon) return;
            
            Weapon.RefillAmmo();
            
        }
    }
}
