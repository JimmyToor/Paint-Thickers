using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles particle interactions with other objects
public class ParticlePainter : MonoBehaviour
{
    public Brush brush;
    public bool randomChannel = false;
    public float damage;

    private ParticleSystem part;
    private List<ParticleCollisionEvent> collisionEvents;

    private void Start()
    {
        part = GetComponent<ParticleSystem>();
        collisionEvents = new List<ParticleCollisionEvent>();
        if (brush.splatTexture == null)
        {
            brush.splatTexture = Resources.Load<Texture2D>("splats");
            brush.splatsX = 4;
            brush.splatsY = 4;
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        switch (other.tag)
        {
            case "Terrain":
                PaintTarget paintTarget = other.GetComponent<PaintTarget>();
                if (paintTarget != null)
                {
                    if (randomChannel) brush.splatChannel = Random.Range(0, 2);

                    int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
                    for (int i = 0; i < numCollisionEvents; i++)
                    {
                        PaintTarget.PaintObject(paintTarget, collisionEvents[i].intersection, collisionEvents[i].normal, brush);
                    }
                }
                break;
            case "Player":
                if (other.TryGetComponent(out Player player))
                {
                    int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
                    for (int i = 0; i < numCollisionEvents; i++)
                    {
                        if (brush.splatChannel != other.GetComponent<Player>().teamChannel)
                        {
                            other.gameObject.GetComponent<PlayerEvents>().OnTakeHit(damage);
                        }
                    }
                }
                break;
            default:
                if (other.gameObject.TryGetComponent(out Health health))
                {
                    part.GetCollisionEvents(other, collisionEvents);
                    health.TakeHit(damage,collisionEvents[0].intersection);
                }
                else
                    Debug.LogFormat("Object {0} hit by particle (fired by {1}) but has no way to react.", other.name, gameObject.name);
                break;
        };
    }
}