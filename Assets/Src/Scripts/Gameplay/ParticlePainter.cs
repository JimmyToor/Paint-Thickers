using System.Collections.Generic;
using Paintz_Free.Scripts;
using Src.Scripts.AI;
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
        public bool collisionSfx;
        [ShowIf(nameof(collisionSfx))]
        public SFXSource sfxSource;


        private ParticleSystem _partSys;
        private List<ParticleCollisionEvent> _collisionEvents;

        private void Start()
        {
            _partSys = GetComponent<ParticleSystem>();
            _collisionEvents = new List<ParticleCollisionEvent>();
        }

        private void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = _partSys.GetCollisionEvents(other, _collisionEvents);
            for (int i = 0; i < numCollisionEvents; i++)
            {
                CheckCollision(_collisionEvents[i], other);
            }
        
            if (collisionSfx)
            {
                for (int i = 0; i < numCollisionEvents; i++)
                {
                    sfxSource.TriggerPlay(_collisionEvents[i].intersection);
                }
            }
        }

        private void CheckCollision(ParticleCollisionEvent collisionEvent, GameObject other)
        {
            switch (LayerMask.LayerToName(other.layer))
            {
                case "Terrain": 
                    SpawnSplash(collisionEvent.normal, collisionEvent.intersection, brush.splatChannel);
                    if (!other.CompareTag("Terrain")) break; // Must be on 'Terrain' layer and have 'Terrain' tag
                    
                    PaintTarget paintTarget = other.GetComponent<PaintTarget>();
                    if (paintTarget != null)
                    {
                        Vector3 normal = collisionEvent.normal;
                        if (randomChannel) brush.splatChannel = Random.Range(0, 2);
                        paintTarget.PaintSphere(collisionEvent.intersection, normal, brush);
                    }
                    break;
                case "Players":
                    if (other.TryGetComponent(out Player player))
                    {
                        if (brush.splatChannel != player.TeamChannel)
                        {
                            other.gameObject.GetComponent<PlayerEvents>().OnTakeHit(damage);
                        }
                    }
                    break;
                case "Enemies":
                    if (other.TryGetComponent(out Enemy enemy))
                    {
                        if (brush.splatChannel != enemy.team.teamChannel && other.gameObject.TryGetComponent(out Health enemyHealth))
                        {
                            enemyHealth.TakeHit(damage,collisionEvent.intersection);
                        }
                    }
                    break;
                default:
                    if (other.gameObject.TryGetComponent(out Health health))
                    {
                        health.TakeHit(damage,collisionEvent.intersection);
                    }
                    // else
                    //     Debug.LogFormat("Object {0} hit by particle (fired by {1}) but has no way to react on layer {2}.",
                    //         other.gameObject, other.layer, gameObject.name);
                    break;
            }
        }

        private void SpawnSplash(Vector3 normal, Vector3 intersection, int channel)
        {
            if (splashObject != null)
            {
                Vector3 tangent = Vector3.Cross(normal, Vector3.up);
                Quaternion rot = Quaternion.identity;
                if (tangent.magnitude > 0.001f)
                    rot = Quaternion.LookRotation(tangent, normal);
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
}