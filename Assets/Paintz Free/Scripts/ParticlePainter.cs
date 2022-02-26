using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handles particle interactions with other objects
public class ParticlePainter : MonoBehaviour
{
    public Brush brush;
    public bool randomChannel = false;
    public int damage;

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
                    if (brush.splatChannel != other.GetComponent<Player>().teamChannel)
                        other.gameObject.GetComponent<PlayerEvents>().TakeDamage(damage);
                }
                break;
            case "Enemy":
                if (other.gameObject.TryGetComponent(out Enemy enemy))
                    enemy.OnHit(damage);
                else
                    Debug.LogErrorFormat("Object hit by particle (fired by {0}) has Enemy tag but is missing Enemy script!", gameObject.name);
                break;
        };
    }
}