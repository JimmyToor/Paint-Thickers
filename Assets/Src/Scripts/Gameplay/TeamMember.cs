using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Src.Scripts.Gameplay
{
    public class TeamMember : MonoBehaviour
    {
        [FormerlySerializedAs("TeamChannel")]
        [Tooltip("The colour channel to identify as friendly paint. [-1, 4].")]
        [Range(-1,4)]
        public int teamChannel;
    }
}
