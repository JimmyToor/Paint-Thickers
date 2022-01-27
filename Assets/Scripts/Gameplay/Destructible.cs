using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Destructible : MonoBehaviour
{
    public int hitsRemaining;
    public bool useHitDamageMaterial = true;
    public bool useHitFX = true;
    public bool useDestroyFX = true;
    public float hitCooldown;
    public Material damageMaterial;
    public GameObject hitFX;
    public GameObject destroyFX;
    
    private Renderer _renderer;
    private bool onCooldown = false;
    private List<ParticleCollisionEvent> collisionEvents;

        
    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<Renderer>();
        collisionEvents = new List<ParticleCollisionEvent>();
    }
    
    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Projectile") && !onCooldown)
        {
            ParticlePhysicsExtensions.GetCollisionEvents(other.GetComponent<ParticleSystem>(), gameObject, collisionEvents);
            TakeHit();
        }
    }

    private void TakeHit()
    {
        hitsRemaining--;
        if (useHitDamageMaterial && _renderer.material != damageMaterial);
            _renderer.material = damageMaterial;

        if (useHitFX)
            Instantiate(hitFX, collisionEvents[0].intersection, Quaternion.identity);

        if (hitsRemaining <= 0)
            OnDeath();

        if (hitCooldown != 0)
            StartCoroutine(StartCooldown());
    }

    public IEnumerator StartCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(hitCooldown);
        onCooldown = false;
    }
    
    void OnDeath()
    {
        if (useDestroyFX)
            Instantiate(destroyFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

}
