using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class LaunchPad : MonoBehaviour
{
    public Transform endTarget;
    public float speed; // Units to travel per step (second)
    public float arcHeight;

    private void OnTriggerStay(Collider other) 
    {
        if (other.gameObject.TryGetComponent(out Launchable launchable) && !launchable.isLaunched && launchable.canLaunch)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                Player player = other.gameObject.GetComponent<Player>();
                if (player.isSquid)
                    player.GetComponent<PlayerEvents>().OnLaunch();
            }
            launchable.Launch(endTarget.position, speed, arcHeight);
        }
    } 
}
