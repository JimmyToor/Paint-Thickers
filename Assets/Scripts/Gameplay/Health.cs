using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float hitpoints;
    public bool invulnerable;
    public bool useHitDamageMaterial = true;
    public bool useHitFX = true;
    public bool useDestroyFX = true;
    public bool destroyOnDeath;
    public float hitCooldown;
    public Material damageMaterial;
    public GameObject hitFX;
    public GameObject destroyFX;
    public UnityEvent onDeath;
    
    private Renderer _renderer;
    private bool onCooldown = false;

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void TakeHit(float damage, Vector3 hitPos = default(Vector3))
    {
        if (useHitDamageMaterial && _renderer.material != damageMaterial)
            _renderer.material = damageMaterial;

        if (useHitFX)
            Instantiate(hitFX, hitPos, Quaternion.identity);

        ReduceHP(damage);

        if (hitCooldown != 0)
            StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(hitCooldown);
        onCooldown = false;
    }

    void OnDeath()
    {
        if (useDestroyFX)
            Instantiate(destroyFX, transform.position, Quaternion.identity);
        
        if (destroyOnDeath)
            Destroy(gameObject);
    }
    
    public void ReduceHP(float damage)
    {
        if (!invulnerable)
        {
            hitpoints -= damage;
            if (hitpoints <= 0)
            {
                onDeath.Invoke();
                OnDeath();
            }
        }
    }
}
