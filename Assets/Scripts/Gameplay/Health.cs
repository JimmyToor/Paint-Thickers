using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    public float hitpoints;
    public float maxHitpoints;
    public bool invulnerable;
    public bool useHitDamageMaterial;
    public bool destroyOnDeath;
    public bool regenerative; // Whether or not health regenerates
    public float hitCooldown; // How long until we can be hit again after being hit
    public float regenCooldown; // How long until health regenerates
    public float regenRate; // How many hitpoints are regenerated per LateUpdate
    public Material damageMaterial;
    public GameObject hitFX;
    public List<AudioClip> hitSFX; // Requires an AudioSource component
    public GameObject deathFX;
    public List<AudioClip> deathSFX; // Requires an AudioSource component
    public UnityEvent onDeath;
    public UnityEvent onHit;

    private Renderer _renderer;
    private bool onCooldown;
    private Animator animator;
    private int takeHitHash = Animator.StringToHash("TakeHit");
    private SFXSource sfxSource;
    private float regenTime; // Time until regeneration starts
    private static ObjectPooler _objectPooler;


    private void Start()
    {
        _renderer = GetComponent<Renderer>();
        TryGetComponent(out animator);
        TryGetComponent(out sfxSource);
        _objectPooler = FindObjectOfType<ObjectPooler>();
    }

    private void Update()
    {
        if (regenerative && hitpoints < maxHitpoints)
        {
            if (regenTime <= 0)
            {
                RegenerateHealth();
            }
            else
            {
                regenTime -= Time.deltaTime;
            }
        }
    }

    private void RegenerateHealth()
    {
        hitpoints += regenRate;
        if (hitpoints > maxHitpoints)
        {
            hitpoints = maxHitpoints;
        }
        regenTime = regenCooldown;
    }

    public void TakeHit(float damage, Vector3 hitPos = default)
    {
        if (onCooldown)
            return;

        if (regenerative)
        {
            regenTime = regenCooldown;
        }
        
        ReduceHP(damage);
        OnHit(hitPos);
        
        if (hitpoints <= 0)
        {
            Debug.Log("dead");
            OnDeath();
        }
        
        if (useHitDamageMaterial && _renderer.material != damageMaterial)
            _renderer.material = damageMaterial;
        
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
        if (deathFX != null)
        {
            GameObject fxObject = _objectPooler.GetObjectFromPool(deathFX.tag);
            fxObject.transform.position = transform.position;
            fxObject.transform.rotation = Quaternion.identity;
            fxObject.SetActive(true);
        }
        
        if (sfxSource != null && deathSFX.Count > 0)
        {
            int randClip = UnityEngine.Random.Range(0, deathSFX.Count);
            sfxSource.TriggerPlayOneShot(transform.position,deathSFX[randClip]);
        }
        
        onDeath?.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject);
    }

    void OnHit(Vector3 hitPos)
    {
        if (animator != null)
            animator.SetTrigger(takeHitHash);

        if (hitFX != null)
        {
            GameObject fxObject = _objectPooler.GetObjectFromPool(hitFX.tag);
            Transform fxTransform = fxObject.transform;
            fxTransform.position = hitPos;
            fxTransform.rotation = Quaternion.identity;
            fxObject.SetActive(true);
        }

        if (sfxSource != null && hitSFX.Count > 0 && hitpoints > 0)
        {
            int randClip = UnityEngine.Random.Range(0, hitSFX.Count);
            sfxSource.TriggerPlayOneShot(transform.position,hitSFX[randClip]);
        }
        
        onHit.Invoke();
    }
    
    public void ReduceHP(float damage)
    {
        if (!invulnerable)
        {
            hitpoints -= damage;
        }
    }
}
