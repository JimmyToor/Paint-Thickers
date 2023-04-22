using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class DamageUIController : MonoBehaviour
    {
        [FormerlySerializedAs("noHealthSize")] [Range(0f,1f)]
        public float noHealthIntensity = 0f;
        [FormerlySerializedAs("maxHealthSize")] [Range(0f,1f)]
        public float maxHealthIntensity = 1f;

        public Material material;
        private static readonly int CenterSize = Shader.PropertyToID("_Intensity");

        void Start()
        {
            material = GetComponent<Image>().material;
        }

        public void UpdateDamageUI(float health)
        {
            float newSize = Mathf.Lerp(noHealthIntensity, maxHealthIntensity, health);
            SetIndicatorIntensity(newSize);
        }

        public void SetIndicatorIntensity(float intensity)
        {
            material.SetFloat(CenterSize, Mathf.Clamp(intensity,0,1));
        }
    }
}
