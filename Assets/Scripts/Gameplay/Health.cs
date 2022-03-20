using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float hitpoints;
    public bool invulnerable;
    public bool useHitDamageMaterial;
    public bool useHitFX;
    public bool useDeathFX;
    public bool destroyOnDeath;
    public float hitCooldown; // How long until we can be hit again after being hit
    public Material damageMaterial;
    public GameObject hitFX;
    public GameObject destroyFX;
    public UnityEvent onDeath;
    public UnityEvent onHit;

    private Renderer _renderer;
    private bool onCooldown;
    private Animator animator;
    private int takeHitHash = Animator.StringToHash("TakeHit");
    

    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        TryGetComponent(out animator);
    }

    public void TakeHit(float damage, Vector3 hitPos = default(Vector3))
    {
        if (onCooldown)
            return;

        OnHit(hitPos);
        
        if (useHitDamageMaterial && _renderer.material != damageMaterial)
            _renderer.material = damageMaterial;

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
        if (useDeathFX)
            Instantiate(destroyFX, transform.position, Quaternion.identity);
        
        onDeath?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    void OnHit(Vector3 hitPos)
    {
        if (animator != null)
            animator.SetTrigger(takeHitHash);
        
        if (useHitFX)
            Instantiate(hitFX, hitPos, Quaternion.identity);
        
        onHit.Invoke();
    }
    
    public void ReduceHP(float damage)
    {
        if (!invulnerable)
        {
            hitpoints -= damage;
            if (hitpoints <= 0)
            {
                OnDeath();
            }
        }
    }
}
