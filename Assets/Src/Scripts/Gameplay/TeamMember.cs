using System;
using Src.Scripts.Gameplay;
using UnityEngine;

namespace Gameplay
{
    public class TeamMember : MonoBehaviour
    {
        [Tooltip("The colour channel to identify as friendly paint. [-1, 4].")]
        [Range(-1,4)]
        public int teamChannel;
    }
}
