using Src.Scripts.Gameplay;
using UnityEngine;

namespace Src.Scripts.AI
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
        
        protected virtual void Awake()
        {
            if (team == null)
            {
                TryGetComponent(out team);
            }
        }

        public void SetupManager()
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
