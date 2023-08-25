using System;
using Src.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Src.Scripts.UI
{
    public class DamageUIController : MonoBehaviour
    {
        [Range(0f,1f)]
        public float noHealthIntensity = 0f;
        [Range(0f,1f)]
        public float maxHealthIntensity = 1f;

        public Health health;

        public Material material;
        private static readonly int CenterSize = Shader.PropertyToID("_Intensity");

        private void Awake()
        {
            material = GetComponent<Image>().material;
        }

        void Start()
        {
            UpdateDamageUI(health.HealthNormalized);
        }

        private void OnEnable()
        {
            health.onHealthChanged += UpdateDamageUI;
        }

        public void UpdateDamageUI(float newHealth)
        {
            float newSize = Mathf.Lerp(noHealthIntensity, maxHealthIntensity, newHealth);
            SetIndicatorIntensity(newSize);
        }

        public void SetIndicatorIntensity(float intensity)
        {
            material.SetFloat(CenterSize, Mathf.Clamp(intensity,0,1));
        }
    }
}
