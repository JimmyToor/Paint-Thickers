using System;
using UnityEngine;

namespace Gameplay
{
    /// <summary>
    /// Enables an enemy group when an object with the Player tag collides with the trigger.
    /// </summary>
    public class SpawnVolume : MonoBehaviour
    {
        public int enemyGroupId;

        private EnemyManager _enemyManager;
        private Vector3 _pos;
        void Start()
        {
            _enemyManager = FindObjectOfType<EnemyManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _enemyManager.EnableGroup(enemyGroupId);
                Destroy(this);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            _pos = transform.position;
            Gizmos.DrawWireCube(_pos, transform.lossyScale);
            Gizmos.DrawIcon(_pos, "Light_Gizmo.png");
        }
    }
}
