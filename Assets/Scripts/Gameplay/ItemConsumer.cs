using System.Collections;
using System.Collections.Generic;
using Gameplay;
using UnityEngine;
using Utility;

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
