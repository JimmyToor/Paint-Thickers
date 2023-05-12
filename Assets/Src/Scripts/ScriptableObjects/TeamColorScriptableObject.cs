using System;
using UnityEngine;

namespace Src.Scripts.ScriptableObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Team Colors Data", menuName = "Data Objects/Team Colors Object")]
    public class TeamColorScriptableObject : ScriptableObject
    {
        public Color[] teamColors;
    }
}
