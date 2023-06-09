using System.Collections;
using System.Collections.Generic;
using Src.Scripts.Audio;
using Src.Scripts.UI;
using Src.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Src.Scripts.Gameplay
{
    public class Health : MonoBehaviour
    {
        public float hitpoints;
        public float maxHitpoints;
        public bool invulnerable;
        public bool useHitDamageMaterial;
        public bool destroyOnDeath;
        public float destroyOnDeathDelay;
        public bool regenerative; // Whether or not health regenerates
        public float hitCooldown; // How long until we can be hit again after being hit
        public float regenCooldown; // How long until health regenerates
        public float regenRate; // How many hitpoints are regenerated per LateUpdate
        public Material damageMaterial;
        public DamageUIController damageUIController;
        public GameObject hitFX;
        public List<AudioClip> hitSfx; // Requires an SFXSource component
        public GameObject deathFX;
        public List<AudioClip> deathSfx; // Requires an SFXSource component
        public UnityEvent onDeath;
        public UnityEvent onHit;

        public float HealthNormalized => hitpoints / maxHitpoints;

        private Renderer _renderer;
        private bool _onCooldown;
        private Animator _animator;
        private int _takeHitHash = Animator.StringToHash("Take Hit");
        private SFXSource _sfxSource;
        private float _regenTime; // Time until regeneration starts
        private bool _updateUI = true;


        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            TryGetComponent(out _animator);
            TryGetComponent(out _sfxSource);
        }

        private void FixedUpdate()
        {
            if (!regenerative || !(hitpoints < maxHitpoints)) return;
            
            if (hitpoints > 0 && _regenTime <= 0)
            {
                RegenerateHealth();
            }
            else
            {
                _regenTime -= Time.deltaTime;
            }
        }

        private void LateUpdate()
        {
            if (damageUIController != null && _updateUI)
            {
                damageUIController.UpdateDamageUI(HealthNormalized);
                _updateUI = false;
            }
        }

        private void RegenerateHealth()
        {
            hitpoints += regenRate;
            if (hitpoints > maxHitpoints)
            {
                hitpoints = maxHitpoints;
            }
            _updateUI = true;
        }

        public void TakeHit(float damage = 1, Vector3 hitPos = default)
        {
            if (_onCooldown)
                return;

            if (hitpoints <= 0)
            {
                // Already dead
                return;
            }
            
            if (regenerative)
            {
                _regenTime = regenCooldown;
            }

            ReduceHP(damage);
            OnHit(hitPos);
        
            if (hitpoints <= 0)
            {
                OnDeath();
            }
        
            if (useHitDamageMaterial && _renderer.material != damageMaterial)
                _renderer.material = damageMaterial;
        
            if (hitCooldown != 0)
                StartCoroutine(StartCooldown());
        }

        private IEnumerator StartCooldown()
        {
            _onCooldown = true;
            yield return new WaitForSeconds(hitCooldown);
            _onCooldown = false;
        }

        [ContextMenu("Trigger Death")]
        void OnDeath()
        {
            if (deathFX != null)
            {
                GameObject fxObject = ObjectPooler.Instance.GetObjectFromPool(deathFX.tag);
                fxObject.transform.position = transform.position;
                fxObject.transform.rotation = Quaternion.identity;
                fxObject.SetActive(true);
            }
        
            if (_sfxSource != null && deathSfx.Count > 0)
            {
                int randClip = Random.Range(0, deathSfx.Count);
                _sfxSource.TriggerPlayOneShot(transform.position,deathSfx[randClip]);
            }
        
            onDeath?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject,destroyOnDeathDelay);
            }
        }

        void OnHit(Vector3 hitPos)
        {
            _regenTime = regenCooldown;
            if (_animator != null)
                _animator.SetTrigger(_takeHitHash);

            if (hitFX != null)
            {
                GameObject fxObject = ObjectPooler.Instance.GetObjectFromPool(hitFX.tag);
                
                if (fxObject == null)
                {
                    Debug.Log("Could not retrieve VFX object from pool.");
                    return;
                }
                
                Transform fxTransform = fxObject.transform;
                fxTransform.position = hitPos;
                fxTransform.rotation = Quaternion.identity;
                fxObject.SetActive(true);
            }

            if (_sfxSource != null && hitSfx.Count > 0)
            {
                int randClip = Random.Range(0, hitSfx.Count);
                _sfxSource.TriggerPlayOneShot(transform.position,hitSfx[randClip]);
            }
        
            onHit.Invoke();
        }
    
        public void ReduceHP(float damage)
        {
            if (!invulnerable)
            {
                hitpoints -= damage;
                _updateUI = true;
            }
        }

        [ContextMenu("Trigger Hit")]
        public void DebugHit()
        {
            TakeHit(1, transform.position);
        }
    }
}
