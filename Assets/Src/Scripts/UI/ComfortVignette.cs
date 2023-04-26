using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Vignette = UnityEngine.Rendering.Universal.Vignette;

namespace UI
{
    public class ComfortVignette : MonoBehaviour
    {

        [SerializeField] private bool vignetteActive;
        [SerializeField] private float intensity;
        [SerializeField] private float fadeDuration;

        private Vignette _vignette;
        private Volume _volume;
        private bool _vignetteActive;
        private Vector3 _oldForward;

        private CharacterController _charController;
        // Start is called before the first frame update
        void OnEnable()
        {
            _oldForward = transform.forward;
            _volume = GameObject.Find("PostProcessVol").GetComponent<Volume>();
            _volume.profile.TryGet(out _vignette);
            _charController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!vignetteActive)
            {
                return;
            }
        
            if (IsMoving() && !vignetteActive) // Fade in vignette if player goes from stationary to moving
            {
                FadeIn();
                vignetteActive = true;
            }
            else if (!IsMoving() && vignetteActive) // Fade out vignette if player goes from moving to stationary
            {
                FadeOut();
                vignetteActive = false;
            }

            _oldForward = transform.forward;
        }

        public void SetIntensity(float newIntensity)
        {
            intensity = newIntensity;
        }

        public void DisableVignette()
        {
            vignetteActive = false;
        }
        
        public void ActivateVignette()
        { 
            vignetteActive = true;
        }
        
        // Is the player rotating or moving?
        private bool IsMoving()
        {
            if (Mathf.Approximately(_charController.velocity.sqrMagnitude, 0) && transform.forward == _oldForward)
                return false;

            return true;
        }

        private void FadeIn()
        {
            DOTween.To(value => _vignette.intensity.Override(value), _vignette.intensity.value, intensity, fadeDuration);
        }

        private void FadeOut()
        {
            DOTween.To(value => _vignette.intensity.Override(value), _vignette.intensity.value, 0, fadeDuration);
        }

    }
}
