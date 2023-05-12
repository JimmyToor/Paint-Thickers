using Src.Scripts.Utility;
using UnityEngine;
using UnityEngine.Events;

namespace Src.Scripts.Gameplay
{
    public class ItemConsumer : MonoBehaviour
    {
        public ItemType itemType;
        public UnityEvent unityEvent;
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Player player = collision.gameObject.GetComponentInParent(typeof(Player)) as Player;
                if (player != null && player.ConsumeItem(itemType))
                {
                    unityEvent.Invoke();
                    Destroy(gameObject);
                }
            }
        }
    }
}
