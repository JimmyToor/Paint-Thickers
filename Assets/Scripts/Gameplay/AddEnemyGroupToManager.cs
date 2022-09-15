using System;
using AI;
using UnityEngine;

namespace Gameplay
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