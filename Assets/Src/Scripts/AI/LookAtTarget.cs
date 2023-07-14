using UnityEngine;
using UnityEngine.Animations;

namespace Src.Scripts.AI
{
    public class LookAtTarget : MonoBehaviour
    {
        public LookAtConstraint lookAtConstraint;

        public void StartLookingAtTarget(Transform trans)
        {
            // Look at the player's head instead of their feet
            if (trans.CompareTag("Player"))
            {
                Transform headTrans = trans.GetChild(0).Find("Main Camera")?.transform;
                if (headTrans != null)
                {
                    trans = headTrans;
                }
            }
            
            lookAtConstraint.AddSource(new ConstraintSource
            {
                sourceTransform = trans,
                weight = 1f
            });
            lookAtConstraint.constraintActive = true;
        }

        /// <summary>
        /// Assumes one source at index 0 and removes it and disables the constraint.
        /// </summary>
        public void StopLookingAtTarget()
        {
            if (lookAtConstraint.sourceCount <= 0) return;
        
            lookAtConstraint.constraintActive = false;
            lookAtConstraint.RemoveSource(0);
        }
    }
}
