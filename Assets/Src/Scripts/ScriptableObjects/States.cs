using System;
using Src.Scripts.AI.States;
using UnityEngine;

namespace Src.Scripts.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "New States Data", menuName = "Data Objects/StatesData")]
    public class States : ScriptableObject
    {
        public StateId[] stateList;
    }
}