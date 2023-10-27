using Src.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Src.Scripts.Gameplay
{
    public class ItemConsumer : MonoBehaviour
    {
        public ItemType itemType;
        public UnityEvent onConsumeItem;
        
        private void OnTriggerEnter(Collider col)
        {
            if (!col.gameObject.CompareTag("Player")) return;
            if (!col.gameObject.TryGetComponent(out Player player)) return;
            if (!player.ConsumeItem(itemType)) return;
            
            ConsumeItem();
        }

        [ContextMenu("Consume Item")]
        private void ConsumeItem()
        {
            onConsumeItem.Invoke();
            GetComponent<Collider>().enabled = false;
        }
    }
}
