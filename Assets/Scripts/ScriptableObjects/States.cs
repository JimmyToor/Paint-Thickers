using System;
using AI;
using UnityEngine;

namespace ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "New States Data", menuName = "Data Objects/StatesData")]
    public class States : ScriptableObject
    {
        public StateId[] stateList;
    }
}