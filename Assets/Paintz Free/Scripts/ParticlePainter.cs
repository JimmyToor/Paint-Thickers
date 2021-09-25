using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePainter : MonoBehaviour
{
    public Brush brush;
    public bool RandomChannel = false;
    public float damage = 1;

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
                    if (RandomChannel) brush.splatChannel = Random.Range(0, 2);

                    int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);
                    for (int i = 0; i < numCollisionEvents; i++)
                    {
                        PaintTarget.PaintObject(paintTarget, collisionEvents[i].intersection, collisionEvents[i].normal, brush);
                    }
                }
                break;
            case "Player":
                if (brush.splatChannel == other.GetComponent<Player>().teamChannel)
                    other.gameObject.GetComponent<PlayerEvents>().TakeDamage(damage);
                break;
        };
    }
}