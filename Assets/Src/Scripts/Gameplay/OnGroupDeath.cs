using UnityEngine;
using UnityEngine.Events;

// Do something when an enemy group dies
namespace Src.Scripts.Gameplay
{
    public class OnGroupDeath : MonoBehaviour
    {
        private static EnemyManager _enemyManager;
        public int groupId;
        public UnityEvent onGroupDeath;
    
        private void Start()
        {
            _enemyManager = FindObjectOfType<EnemyManager>();
            if (onGroupDeath != null)
            {
                _enemyManager.SubscribeGroupDefeatedEvent(groupId, onGroupDeath.Invoke);
            }
        }

        [ContextMenu("Trigger Events")]
        public void ManualTrigger()
        {
            onGroupDeath.Invoke();
        }
    
    }
}
