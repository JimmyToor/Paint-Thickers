using Src.Scripts.AI;
using UnityEngine;

namespace Src.Scripts.Gameplay
{
    public class AddEnemyGroupToManager : MonoBehaviour
    {
        private void Start()
        {
            AddChildrenToEnemyManager();
        }

        private void AddChildrenToEnemyManager()
        {
            foreach (var enemy in GetComponentsInChildren<Enemy>(true))
            {
                enemy.SetupManager();
            }
        }
    }
}