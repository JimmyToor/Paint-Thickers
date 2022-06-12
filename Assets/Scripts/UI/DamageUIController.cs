using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DamageUIController : MonoBehaviour
    {
        [Range(0f,1f)]
        public float noHealthSize = 0.34f;
        [Range(0f,1f)]
        public float maxHealthSize = 1f;

        private Material material;
        private static readonly int CenterSize = Shader.PropertyToID("_CenterSize");

        // Start is called before the first frame update
        void Start()
        {
            material = GetComponent<Image>().material;
        }

        public void UpdateDamageUI(float health)
        {
            float newSize = Mathf.Lerp(noHealthSize, maxHealthSize, health);
            SetIndicatorIntensity(newSize);
        }

        public void SetIndicatorIntensity(float intensity)
        {
            material.SetFloat(CenterSize, Mathf.Clamp(intensity,0,1));
        }
    }
}
