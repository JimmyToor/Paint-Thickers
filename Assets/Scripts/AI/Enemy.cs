using Gameplay;
using Unity.Collections;
using UnityEngine;
using Utility;

namespace AI
{
    [RequireComponent(typeof(TeamMember))]
    public abstract class Enemy : MonoBehaviour
    {
        public int groupId; // Associate this enemy with a group of enemies
        public ScriptableObjects.States statesData;
        [HideInInspector]
        public TeamMember team;

        private Health _health;
        private static EnemyManager _enemyManager;
        
        protected virtual void Start()
        {
            if (team == null)
            {
                TryGetComponent(out team);
            }
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
