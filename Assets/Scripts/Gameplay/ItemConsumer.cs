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
        if (collision.gameObject.TryGetComponent(out Player player) && player.inventory.ConsumeItem(itemType))
        {
            Destroy(gameObject);
        }
    }
}
