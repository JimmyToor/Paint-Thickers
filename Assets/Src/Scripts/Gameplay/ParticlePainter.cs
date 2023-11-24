using System.Collections.Generic;
using Src.Scripts.Attributes;
using Src.Scripts.Audio;
using Src.Scripts.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Src.Scripts.Gameplay
{
    /// <summary>
    /// Handles particle interactions with other objects
    /// </summary>
    [RequireComponent(typeof(ParticleSystem))]
    public class ParticlePainter : MonoBehaviour
    {
        public Brush brush;
        public float damage;
        public bool randomChannel;
        public GameObject splashObject;
        [Tooltip("Play sound effects on collision (requires SFXSource component)")]
        public bool useCollisionSfx;
        [ShowIf(nameof(useCollisionSfx))]
        public SFXSource sfxSource;


        private ParticleSystem _partSys;
        private List<ParticleCollisionEvent> _collisionEvents;

        private void Awake()
        {
            _partSys = GetComponent<ParticleSystem>();
            _collisionEvents = new List<ParticleCollisionEvent>();
        }

        private void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = _partSys.GetCollisionEvents(other, _collisionEvents);
            for (int i = 0; i < numCollisionEvents; i++)
            {
                if (CheckCollision(_collisionEvents[i] ,other) && useCollisionSfx)
                {
                    sfxSource.TriggerPlay(_collisionEvents[i].intersection);
                }
            }
        
            
        }

        /// <summary>
        /// Checks for specific collisions and performs the required behaviours.
        /// </summary>
        /// <param name="collisionEvent"></param>
        /// <param name="other"></param>
        /// <returns>True when the the collision was successfully acted upon. False otherwise.</returns>
        private bool CheckCollision(ParticleCollisionEvent collisionEvent, GameObject other)
        {
            switch (LayerMask.LayerToName(other.layer))
            {
                case "Terrain": 
                    SpawnSplash(collisionEvent.normal, collisionEvent.intersection, brush.splatChannel);
                    if (!other.CompareTag("Terrain")) break; // Must be on 'Terrain' layer and have 'Terrain' tag
                    
                    if (other.TryGetComponent(out PaintTarget paintTarget))
                    {
                        Vector3 normal = collisionEvent.normal;
                        if (randomChannel) brush.splatChannel = Random.Range(0, 2);
                        paintTarget.PaintSphere(collisionEvent.intersection, normal, brush);
                        return true;
                    }
                    break;
                default:
                    if (!other.gameObject.TryGetComponent(out Health targetHealth))
                    {
                        break;
                    }

                    if (other.TryGetComponent(out TeamMember teamMember) && 
                        brush.splatChannel == teamMember.teamChannel)
                    {
                        break;
                    }
                    
                    targetHealth.TakeHit(damage,collisionEvent.intersection);;
                    return true;
            }
            return false;
        }

        private void SpawnSplash(Vector3 normal, Vector3 intersection, int channel)
        {
            if (splashObject == null) return;
            
            Vector3 tangent = Vector3.Cross(normal, Vector3.up);
            
            Quaternion rot = Quaternion.identity;
            if (tangent.magnitude > 0.001f)
            {
                rot = Quaternion.LookRotation(tangent, normal);
            }
            GameObject splash = ObjectPooler.Instance.GetObjectFromPool(splashObject.tag);
            splash.transform.position = intersection;
            splash.transform.rotation = rot;
            
            if (splash.TryGetComponent(out PaintColorManager manager))
            {
                manager.UpdateColorChannel(channel);
            }
            splash.SetActive(true);
        }
    }
}