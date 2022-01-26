using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    public Transform endTarget;

    private void OnTriggerStay(Collider other) 
    {
        if (other.gameObject.TryGetComponent(out Launchable launchable) && !launchable.IsLaunched)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Player player = other.gameObject.GetComponent<Player>();
                if (player.IsSquid)
                    player.GetComponent<PlayerEvents>().Launch(endTarget.position);
            }
            else
            {
                launchable.Launch(endTarget.position);
            }
        }
    } 
}
