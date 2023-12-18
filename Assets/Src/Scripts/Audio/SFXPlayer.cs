using System.Collections.Generic;
using Src.Scripts.Utility;
using UnityEngine;

// Got this from SO but can't find the source anymore
namespace Src.Scripts.Audio
{
    /// <summary>
    /// Central entry point to play single use SFX (e.g. contact sound). Through a single function call, PlaySFX, allow to
    /// play a given clip at a given point with the given parameters.
    /// The system also support using ID for event, which allow to avoid 2 event with the same ID to play too soon (e.g.
    /// contact sound would overlapping when the object collide multiple time in a second)
    /// </summary>
    public class SFXPlayer : Singleton<SFXPlayer>
    {
        public struct PlayParameters
        {
            public float Pitch;
            public float Volume;
            public int SourceID;
        }

        public class PlayEvent
        {
            public float Time;
        }

        public AudioSource sfxReferenceSource;
        public int sfxSourceCount;

        Dictionary<int, PlayEvent> _mPlayEvents = new Dictionary<int, PlayEvent>();
        List<int> _mPlayingSources = new List<int>();
    
        AudioSource[] _mSfxSourcePool;
    
        int _mUsedSource = 0;


        private void Start()
        {
            _mSfxSourcePool = new AudioSource[sfxSourceCount];

            for (int i = 0; i < sfxSourceCount; ++i)
            {
                _mSfxSourcePool[i] = Instantiate(sfxReferenceSource,transform, true);
                _mSfxSourcePool[i].gameObject.SetActive(false);
            }
        }

        void Update()
        {
            List<int> idToRemove = new List<int>();
            foreach (var playEvent in _mPlayEvents)
            {
                playEvent.Value.Time -= Time.deltaTime;
            
                if(playEvent.Value.Time <= 0.0f)
                    idToRemove.Add(playEvent.Key);
            }

            foreach (var id in idToRemove)
            {
                _mPlayEvents.Remove(id);
            }

            for (int i = 0; i < _mPlayingSources.Count; ++i)
            {
                int id = _mPlayingSources[i];
                if (!_mSfxSourcePool[id].isPlaying)
                {
                    _mSfxSourcePool[id].gameObject.SetActive(false);
                }
            
                _mPlayingSources.RemoveAt(i);
                i--;
            }
        }
    
        //This will return a new source based on the SFX Reference Source, useful for script that want to control their own
        //source but still keep all the settings from the reference (e.g. the mixer)
        //NOTE : caller need to clean that source as the SFXPlayer does not keep any track of it
        public AudioSource GetNewSource()
        {
            return Instantiate(sfxReferenceSource);
        }

        /// <summary>
        /// Will play the given clip at the given place. The Parameter contain a SourceID that allow to uniquely identify
        /// an event so it have to wait the given cooldown time before being able to be played again (e.g. useful for
        /// collision sound, as physic system could create multiple collisions in a short time that would lead to stutter)
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        /// <param name="parameters"></param>
        /// <param name="cooldownTime">Time before another PlaySFX with the same parameters.SourceID can be played again</param>
        public void PlaySfx(AudioClip clip, Vector3 position, PlayParameters parameters, float cooldownTime = 0.5f)
        {
            if(clip == null)
                return;
        
            //can't play this sound again as the previous one with the same source was too early
            if (_mPlayEvents.ContainsKey(parameters.SourceID))
                return;
        
            AudioSource s = _mSfxSourcePool[_mUsedSource];
        
            _mPlayingSources.Add(_mUsedSource);
        
            _mUsedSource = _mUsedSource + 1;
            if (_mUsedSource >= _mSfxSourcePool.Length) _mUsedSource = 0;

            s.gameObject.SetActive(true);
            s.transform.position = position;
            s.clip = clip;

            s.volume = parameters.Volume;
            s.pitch = parameters.Pitch;
        
            _mPlayEvents.Add(parameters.SourceID, new PlayEvent() { Time = cooldownTime });
        
            s.Play();
        }
    }
}
