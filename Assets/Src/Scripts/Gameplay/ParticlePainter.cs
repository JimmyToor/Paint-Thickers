﻿using System.Collections.Generic;
using AI;
using Audio;
using Gameplay;
using Src.Scripts;
using Src.Scripts.Gameplay;
using UnityEngine;

/// <summary>
/// Handles particle interactions with other objects
/// </summary>
public class ParticlePainter : MonoBehaviour
{
    public Brush brush;
    public float damage;
    public bool randomChannel;
    public bool collisionSfx; // play sound effects on collision (requires SFXSource component)

    private ParticleSystem _partSys;
    private List<ParticleCollisionEvent> _collisionEvents;
    private SFXSource _sfxSource;

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
            switch (other.tag)
            {
                case "Terrain":
                    PaintTarget paintTarget = other.GetComponent<PaintTarget>();
                    if (paintTarget != null)
                    {
                        if (randomChannel) brush.splatChannel = Random.Range(0, 2);
                        PaintTarget.PaintSphere(_collisionEvents[i].intersection, _collisionEvents[i].normal, brush);
                        //PaintTarget.PaintObject(paintTarget, _collisionEvents[i].intersection, _collisionEvents[i].normal, brush);
                    }
                    break;
                case "Player":
                    if (other.TryGetComponent(out Player player))
                    {
                        if (brush.splatChannel != player.teamChannel)
                        {
                            other.gameObject.GetComponent<PlayerEvents>().OnTakeHit(damage);
                        }
                    }
                    break;
                case "Enemy":
                    if (other.TryGetComponent(out Enemy enemy))
                    {
                        if (brush.splatChannel != enemy.team.teamChannel && other.gameObject.TryGetComponent(out Health enemyHealth))
                        {
                            enemyHealth.TakeHit(damage,_collisionEvents[i].intersection);
                        }
                    }
                    break;
                default:
                    if (other.gameObject.TryGetComponent(out Health health))
                    {
                        health.TakeHit(damage,_collisionEvents[i].intersection);
                    }
                    // else
                    //     Debug.LogFormat("Object {0} hit by particle (fired by {1}) but has no way to react.", other.name, gameObject.name);
                    break;
            }
        }
        
        if (collisionSfx)
        {
            for (int i = 0; i < numCollisionEvents; i++)
            {
                _sfxSource.TriggerPlay(_collisionEvents[i].intersection);
            }
        }
    }
}