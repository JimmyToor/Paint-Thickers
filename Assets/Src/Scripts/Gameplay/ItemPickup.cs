using Src.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Src.Scripts.Gameplay
{
    public class ItemPickup : MonoBehaviour
    {
        public ItemType itemType;
        public GameObject pickupVfx;
        public UnityEvent pickupEvents;
        
        private Animator _animator;
        private readonly int _pickupHash = Animator.StringToHash("Pickup");
        private void Start()
        {
            TryGetComponent(out _animator);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Player player = collision.gameObject.GetComponentInParent(typeof(Player)) as Player;
                if (player != null)
                {
                    player.AddItem(itemType);
                    Pickup();
                }
            }
        }
        
        [ContextMenu("Trigger Pickup")]
        public void DebugPickup()
        {
            Pickup();
        }

        private void Pickup()
        {
            if (_animator == null)
            {
                Destroy(gameObject);
            }
            else
            {   // Destroy after animation finishes
                _animator.SetTrigger(_pickupHash);
                Destroy(gameObject, _animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }

        public void SpawnVFX()
        {
            var fxObject = ObjectPooler.Instance.GetObjectFromPool(pickupVfx.tag);
            fxObject.transform.position = transform.position;
            fxObject.transform.rotation = Quaternion.identity;
            fxObject.SetActive(true);
        }
    }
}
