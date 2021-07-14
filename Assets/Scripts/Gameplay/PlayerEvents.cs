using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine;
using UnityEngine.Events;

public class PlayerEvents : MonoBehaviour
{
    public InputActionAsset inputs;
    public UnityAction OnSwim;
    public UnityAction OnStand;
    public UnityAction<Vector3> OnMove;
    
    // Start is called before the first frame update
    void Start()
    {
        OnSwim?.Invoke();
        inputs.FindAction("Swim").performed += ctx => OnSwim?.Invoke();
        inputs.FindAction("Swim").canceled += ctx => OnStand?.Invoke();
        inputs.FindAction("Move").performed += ctx => OnMove?.Invoke(new Vector3(ctx.ReadValue<Vector2>().x, 0f, ctx.ReadValue<Vector2>().y));
    }
}
