using UnityEngine;

namespace Src.Scripts.AI
{
    public class TargetModifier : MonoBehaviour
    {
        [Tooltip("When targeted by a scanner, Target Transform will be treated as the target position.")]
        public Transform targetTransform;
    }
}
