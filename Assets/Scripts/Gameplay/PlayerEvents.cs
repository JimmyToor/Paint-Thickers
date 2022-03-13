using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerEvents : MonoBehaviour
{
    Player player;
    public InputActionAsset inputs;
    public Action Land;
    public Action Swim;
    public Action Stand;
    public Action<Vector3> Move;
    public Action<Vector3> Launch;
    public Action<float> TakeHit;
    
    void Start()
    {
        player = GetComponent<Player>();
        inputs.FindAction("Swim").performed += OnSwim;
        inputs.FindAction("Swim").canceled += OnStand;
        inputs.FindAction("Move").performed +=  OnMove;
    }

    private void OnDisable() 
    {
        inputs.FindAction("Swim").performed -= OnSwim;
        inputs.FindAction("Swim").canceled -= OnStand;
        inputs.FindAction("Move").performed -= OnMove;
    }

    private void OnSwim(InputAction.CallbackContext ctx)
    {
        Swim?.Invoke();
    }

    private void OnStand(InputAction.CallbackContext ctx)
    {
        Stand?.Invoke();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Move?.Invoke(new Vector3(ctx.ReadValue<Vector2>().x, 0f, ctx.ReadValue<Vector2>().y));
    }

    public void OnLaunch(Vector3 endPos)
    {   
        player.DisableInputMovement();
        Stand?.Invoke();
        Launch?.Invoke(endPos);
    }

    public void OnLand()
    {
        player.EnableInputMovement();
        Land?.Invoke();
    }

    public void OnTakeHit(float damage)
    {
        TakeHit?.Invoke(damage);
    }
}
