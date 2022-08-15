using Gameplay;
using UnityEngine;
using Utility;

namespace AI
{
    public abstract class Enemy : MonoBehaviour
    {
        public int groupId; // Associate this enemy with a group of enemies
        [Tooltip("The colour channel to identify as friendly paint. [-1, 4].")]
        [Range(-1,4)]
        public int teamChannel;
        public ScriptableObjects.States statesData;

        private Health _health;
        private static EnemyManager _enemyManager;



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
            
            if (TryGetComponent(out _health))
            {
                _health.onDeath.AddListener(RemoveFromManager);
            }
        }

        private void RemoveFromManager()
        {
            _enemyManager.RemoveEnemy(groupId,this);
        }
    }
}
