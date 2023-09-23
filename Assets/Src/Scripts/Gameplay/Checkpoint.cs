using UnityEngine;

namespace Src.Scripts.Gameplay
{
    public class Checkpoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.SetCheckpoint(this);
            }
        }
    }
}
