using DG.Tweening;
using UnityEngine;

public class DirectionTween : MonoBehaviour
{
    public Transform endPos;
    public float duration;

    public void StartTween()
    {
        transform.DOMove(endPos.position, duration);
    }
}
