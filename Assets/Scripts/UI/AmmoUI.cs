using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class AmmoUI : MonoBehaviour
    {
        public Image fillImage;

        private Material mat;

        private void Start()
        {
            mat = fillImage.material;
        }

        public void SetAmount(float amount)
        {
            fillImage.fillAmount = amount;
        }

        public void SetColor(Color color)
        {
            mat.color = color;
        }
    }
}
