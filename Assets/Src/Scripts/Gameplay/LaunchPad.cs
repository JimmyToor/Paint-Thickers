using UnityEngine;

namespace Src.Scripts.Gameplay
{
    public class LaunchPad : MonoBehaviour
    {
        public Transform target;
        public float speed;
        public Launchable.LaunchableParams launchParameters;
        public AudioClip launchAudioClip;
    
        private Vector3 _endPos; 
        private float _stepScale; // Determines duration of launch (larger stepScale = shorter launch)
    
        private void Start()
        {
            launchParameters.endPos = target.position;
        }

        private void OnTriggerStay(Collider other)
        {
            GameObject otherObject = other.gameObject;
            if (otherObject.TryGetComponent(out Launchable launchable) && !launchable.isLaunched && launchable.canLaunch)
            {
                float distance = Vector3.Distance(launchable.transform.position, launchParameters.endPos);
                launchParameters.stepScale = speed / distance;
                AudioSource.PlayClipAtPoint(launchAudioClip,transform.position);
                launchable.Launch(launchParameters);
            }
        }
    }
}
