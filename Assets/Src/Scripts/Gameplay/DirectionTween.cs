using DG.Tweening;
using UnityEngine;

namespace Src.Scripts.Gameplay
{
    public class DirectionTween : MonoBehaviour
    {
        public Transform endPos;
        public float duration;
        private Vector3 _startPos;

        private void Start()
        {
            _startPos = transform.position;
        }

        public void StartTween()
        {
            transform.DOMove(endPos.position, duration);
        }

        public void ResetPosition()
        {
            transform.position = _startPos;
        }
    }
}
