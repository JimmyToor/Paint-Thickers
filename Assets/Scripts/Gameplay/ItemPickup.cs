using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class ItemPickup : MonoBehaviour
{
    public ItemType itemType;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.TryGetComponent(out Player player);
            if (player != null)
            {
                player.inventory.AddItem(itemType);
                Destroy(gameObject);
            }
        }
    }
}
