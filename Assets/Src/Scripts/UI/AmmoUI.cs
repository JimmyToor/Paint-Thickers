using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Scripts.UI
{
    public class AmmoUI : MonoBehaviour
    {
        public Weapon weapon;
        public Image fillImage;
        public Image fillCap;
        public Canvas canvas;
        
        private void OnEnable()
        {
            weapon.onColorChanged += SetColor;
            weapon.onAmmoNormalizedChanged += SetAmount;
            weapon.onWeaponEquipped += ShowUI;
            weapon.onWeaponUnequipped += HideUI;
        }
        
        private void OnDisable()
        {
            weapon.onColorChanged -= SetColor;
            weapon.onAmmoNormalizedChanged -= SetAmount;
            weapon.onWeaponEquipped -= ShowUI;
            weapon.onWeaponUnequipped -= HideUI;
        }

        public void SetAmount(float amount)
        {
            fillImage.fillAmount = amount;
            fillCap.fillAmount = amount;
        }

        public void SetColor(Color color)
        {
            fillImage.material.color = color;
        }

        public void HideUI()
        {
            canvas.enabled = false;
        }

        public void ShowUI()
        {
            canvas.enabled = true;
        }
    }
}
