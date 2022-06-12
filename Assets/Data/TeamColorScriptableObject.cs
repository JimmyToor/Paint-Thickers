using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "New Team Colors Data", menuName = "Data Objects/Team Colors Object")]
    public class TeamColorScriptableObject : ScriptableObject
    {
        public Color teamColor1;
        public Color teamColor2;
        public Color teamColor3;
        public Color teamColor4;
    }
}
