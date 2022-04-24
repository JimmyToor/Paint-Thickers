using UnityEngine;
using Utility;

namespace AI
{
    public class Enemy : MonoBehaviour
    {
        private static GameManager _gameManager;
    
        protected Health health;
    
        public int groupId; // Associate this enemy with a group of enemies

        protected virtual void Start()
        {
            SetupManager();
        }
    
        private void SetupManager()
        {
            _gameManager = FindObjectOfType<GameManager>();
            if (_gameManager == null)
            {
                Debug.Log("No GameManager found in the scene!");
                return;
            }
            _gameManager.AddEnemy(groupId,this);
            
            if (TryGetComponent(out health))
            {
                health.onDeath.AddListener(RemoveFromManager);
            }
        }

        private void RemoveFromManager()
        {
            _gameManager.RemoveEnemy(groupId,this);
        }
    }
}
