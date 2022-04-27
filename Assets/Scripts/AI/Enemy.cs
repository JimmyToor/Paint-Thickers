using UnityEngine;
using Utility;

namespace AI
{
    public class Enemy : MonoBehaviour
    {
        private static EnemyManager _enemyManager;
    
        protected Health health;
    
        public int groupId; // Associate this enemy with a group of enemies

        protected virtual void Start()
        {
            SetupManager();
        }
    
        private void SetupManager()
        {
            _enemyManager = FindObjectOfType<EnemyManager>();
            if (_enemyManager == null)
            {
                Debug.Log("No EnemyManager found in the scene!");
                return;
            }
            _enemyManager.AddEnemy(groupId,this);
            
            if (TryGetComponent(out health))
            {
                health.onDeath.AddListener(RemoveFromManager);
            }
        }

        private void RemoveFromManager()
        {
            _enemyManager.RemoveEnemy(groupId,this);
        }
    }
}
