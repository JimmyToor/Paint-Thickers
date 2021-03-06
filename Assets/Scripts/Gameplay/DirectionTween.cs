using System;
using DG.Tweening;
using UnityEngine;

public class DirectionTween : MonoBehaviour
{
    public Transform endPos;
    public float duration;
    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    public void StartTween()
    {
        transform.DOMove(endPos.position, duration);
    }

    public void ResetPosition()
    {
        transform.position = startPos;
    }
}
