using Src.Scripts.Gameplay;
using UnityEngine;

namespace Src.Scripts.Utility
{
    public class RespawnVolume : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            ResetPlayerPosition(other);
        }

        private void OnTriggerStay(Collider other)
        {
            ResetPlayerPosition(other);
        }

        private static void ResetPlayerPosition(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<Player>().ResetPosition();
            }
        }
    }
}
