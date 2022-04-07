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
            Player player = collision.gameObject.GetComponentInParent(typeof(Player)) as Player;
            if (player != null)
            {
                player.AddItem(itemType);
                Destroy(gameObject);
            }
        }
    }
}
