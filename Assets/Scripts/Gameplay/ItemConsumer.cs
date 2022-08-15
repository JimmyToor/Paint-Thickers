using UnityEngine;
using Utility;

namespace Gameplay
{
    public class ItemConsumer : MonoBehaviour
    {
        public ItemType itemType;
    
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Player player = collision.gameObject.GetComponentInParent(typeof(Player)) as Player;
                if (player != null && player.ConsumeItem(itemType))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
