using UnityEngine;

namespace Src.Scripts.Gameplay
{
    public class Checkpoint : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Player _))
            {
                GameManager.Instance.NewCheckpoint(this);
            }
        }
    }
}
