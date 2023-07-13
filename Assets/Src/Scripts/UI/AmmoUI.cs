using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Scripts.UI
{
    public class AmmoUI : MonoBehaviour
    {
        public Weapon weapon;
        public Image fillImage;
        public Canvas canvas;

        private Material _mat;

        private void OnEnable()
        {
            _mat = fillImage.material;
            weapon.onColorChanged += SetColor;
            weapon.onAmmoNormalizedChanged += SetAmount;
            weapon.onWeaponEquipped += ShowUI;
            weapon.onWeaponUnequipped += HideUI;
        }
        
        private void OnDisable()
        {
            _mat = fillImage.material;
            weapon.onColorChanged -= SetColor;
            weapon.onAmmoNormalizedChanged -= SetAmount;
            weapon.onWeaponEquipped -= ShowUI;
            weapon.onWeaponUnequipped -= HideUI;
        }

        public void SetAmount(float amount)
        {
            fillImage.fillAmount = amount;
        }

        public void SetColor(Color color)
        {
            _mat.color = color;
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
