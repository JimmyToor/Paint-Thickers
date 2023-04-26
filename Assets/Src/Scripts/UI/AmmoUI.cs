using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AmmoUI : MonoBehaviour
    {
        public Image fillImage;

        private Material _mat;

        private void OnEnable()
        {
            _mat = fillImage.material;
        }

        public void SetAmount(float amount)
        {
            fillImage.fillAmount = amount;
        }

        public void SetColor(Color color)
        {
            _mat.color = color;
        }
    }
}
