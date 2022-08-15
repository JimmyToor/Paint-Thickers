using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using Vignette = UnityEngine.Rendering.Universal.Vignette;

namespace UI
{
    public class ComfortVignette : MonoBehaviour
    {
        public bool enableVignette;
        public float intensity;
        public float fadeDuration;

        private Vignette vignette;
        private Volume volume;
        private bool isMoving;
        private Vector3 oldForward;

        private CharacterController charController;
        // Start is called before the first frame update
        void Awake()
        {
            oldForward = transform.forward;
            volume = GameObject.Find("PostProcessVol").GetComponent<Volume>();
            volume.profile.TryGet(out vignette);
            charController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!enableVignette)
            {
                return;
            }
        
            if (Moving() && !isMoving) // Fade in vignette if player goes from stationary to moving
            {
                FadeIn();
                isMoving = true;
            }
            else if (!Moving() && isMoving) // Fade out vignette if player goes from moving to stationary
            {
                FadeOut();
                isMoving = false;
            }

            oldForward = transform.forward;
        }

        // Is the player rotating or moving?
        private bool Moving()
        {
            if (Mathf.Approximately(charController.velocity.sqrMagnitude, 0) && transform.forward == oldForward)
                return false;

            return true;
        }

        private void FadeIn()
        {
            DOTween.To(value => vignette.intensity.Override(value), vignette.intensity.value, intensity, fadeDuration);
        }

        private void FadeOut()
        {
            DOTween.To(value => vignette.intensity.Override(value), vignette.intensity.value, 0, fadeDuration);
        }
    
    }
}
