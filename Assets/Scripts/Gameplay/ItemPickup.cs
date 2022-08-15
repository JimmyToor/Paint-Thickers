using UnityEngine;
using Utility;

namespace Gameplay
{
    public class ItemPickup : MonoBehaviour
    {
        public ItemType itemType;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Player player = collision.gameObject.GetComponentInParent(typeof(Player)) as Player;
                if (player != null)
                {
                    player.AddItem(itemType);
                    Destroy(gameObject);
                }
            }
        }
    }
}
