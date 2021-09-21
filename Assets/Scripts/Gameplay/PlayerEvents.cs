using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine;

public class PlayerEvents : MonoBehaviour
{
    Player player;
    public InputActionAsset inputs;
    public event Action OnLand;
    public event Action OnSwim;
    public event Action OnStand;
    public event Action<Vector3> OnMove;
    public event Action<Vector3> OnLaunch;
    
    void Start()
    {
        player = GetComponent<Player>();
        inputs.FindAction("Swim").performed += Swim;
        inputs.FindAction("Swim").canceled += Stand;
        inputs.FindAction("Move").performed +=  Move;
    }

    private void OnDisable() 
    {
        inputs.FindAction("Swim").performed -= Swim;
        inputs.FindAction("Swim").canceled -= Stand;
        inputs.FindAction("Move").performed -= Move;
    }

    private void Swim(InputAction.CallbackContext ctx)
    {
        OnSwim?.Invoke();
    }

    private void Stand(InputAction.CallbackContext ctx)
    {
        OnStand?.Invoke();
    }

    private void Move(InputAction.CallbackContext ctx)
    {
        OnMove?.Invoke(new Vector3(ctx.ReadValue<Vector2>().x, 0f, ctx.ReadValue<Vector2>().y));
    }

    public void Launch(Vector3 endPos)
    {   
        player.DisableInputMovement();
        OnStand?.Invoke();
        OnLaunch?.Invoke(endPos);
    }

    public void Land()
    {
        player.EnableInputMovement();
        OnLand?.Invoke();
    }
    
    
}
