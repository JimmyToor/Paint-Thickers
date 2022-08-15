using UnityEngine;
using Random = UnityEngine.Random;

// Got this from SO but can't find the post anymore

namespace Audio
{
    /// <summary>
    /// Will trigger SFX through the SFXPlayer when the object on which this is added trigger a collision enter event
    /// </summary>
    public class SFXSource : MonoBehaviour
    {
        static int _sIDMax;

        public AudioClip[] clips;
        public float volume = 1f;
        public float minPitch = 1.2f;
        public float maxPitch = 0.8f;
        public float cooldownTime;

        private int _mID;

        void Awake()
        {
            _mID = _sIDMax;
            _sIDMax++;
        }

        public void TriggerPlay(Vector3 pos)
        {   
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
        
            SFXPlayer.Instance.PlaySfx(randomClip, pos, new SFXPlayer.PlayParameters()
            {
                Volume = volume,
                Pitch = Random.Range(minPitch, maxPitch),
                SourceID = _mID
            }, cooldownTime);
        }

        public void TriggerPlayOneShot(Vector3 pos, AudioClip clip)
        {
            SFXPlayer.Instance.PlaySfx(clip, pos, new SFXPlayer.PlayParameters()
            {
                Volume = volume,
                Pitch = Random.Range(minPitch, maxPitch),
                SourceID = _mID
            }, cooldownTime);
        }
    }
}