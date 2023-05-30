using Src.Scripts.Gameplay;
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

        public bool IsUnequipValid(Weapon weapon)
        {
            // Only un-equip the weapon if neither hand is holding it.
            if (!weapon.TryGetComponent(out XRBaseInteractable interactable)) return false;
            return weapon == Weapon && leftInteractor.selectTarget != interactable &&
                   rightInteractor.selectTarget != interactable;
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
            if (Weapon == null) return;
            Weapon.ShowUI();
        }

        public void SetUIColor(Color color)
        {
            if (Weapon == null) return;
            Weapon.SetUIColor(color);
        }
        
        public void UnequipWeapon()
        {
            Weapon = null;
        }

        public void DisableWeapon()
        {
            if (Weapon == null) return;
            
            Weapon.gameObject.SetActive(false);
        }

        public void EnableWeapon()
        {
            if (Weapon == null) return;
            
            Weapon.gameObject.SetActive(true);
        }

        public void HideWeapon()
        {
            if (Weapon == null) return;

            Weapon.HideWeapon();
        }

        public void ShowWeapon()
        {
            if (Weapon == null) return;
            Weapon.ShowWeapon();
        }

        public void EnableWeaponUI()
        {
            if (Weapon == null) return;
            
            Weapon.ShowUI();
        }

        public void DisableWeaponUI()
        {
            if (Weapon == null) return;
            
            Weapon.HideUI();
        }
    
        public void RefillWeaponAmmo()
        {
            if (Weapon == null) return;
            Weapon.RefillAmmo();
        }

        public void HumanMode()
        {
            if (Weapon == null) return;
            ShowWeapon();
            Weapon.hideUIAboveThreshold = true;
            Weapon.StopReloadSfx();
        }

        public void SquidMode()
        {
            if (Weapon == null) return;
            HideWeapon();
            Weapon.hideUIAboveThreshold = false;
        }
        
        public void MatchColors(PaintColorMatcher paintColorMatcher)
        {
            // Add the weapon's particle renderers to the color matcher list to ensure they are the correct color
            foreach (var rend in Weapon.renderers)
            {
                if (rend.TryGetComponent(out PaintColorManager colorManager) &&
                    !paintColorMatcher.matchTeamColor.Contains(colorManager))
                {
                    paintColorMatcher.matchTeamColor.Add(colorManager);
                }
            }
            paintColorMatcher.UpdateTeamColor();
        }

        public void UnmatchColors(PaintColorMatcher paintColorMatcher)
        {
            foreach (var rend in Weapon.renderers)
            {
                if (rend.TryGetComponent(out PaintColorManager colorManager))
                {
                    paintColorMatcher.matchTeamColor.Remove(colorManager);
                }
            }
        }
    }
}