using UnityEngine;

namespace Src.Scripts.Utility
{
    public class RespawnVolume : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.ResetPlayerPosition();
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.ResetPlayerPosition();
            }
        }
    }
}
