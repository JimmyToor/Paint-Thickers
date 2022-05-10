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
    public Action Squid;
    public Action Stand;
    public Action<Vector3> Move;
    public Action Launch;
    public Action<float> TakeHit;
    
    void Start()
    {
        player = GetComponent<Player>();
        inputs.FindAction("Squid").performed += OnSquid;
        inputs.FindAction("Squid").canceled += OnStand;
        inputs.FindAction("Move").performed +=  OnMove;
    }

    private void OnDisable() 
    {
        inputs.FindAction("Squid").performed -= OnSquid;
        inputs.FindAction("Squid").canceled -= OnStand;
        inputs.FindAction("Move").performed -= OnMove;
    }

    private void OnSquid(InputAction.CallbackContext ctx)
    {
        Squid?.Invoke();
    }

    private void OnStand(InputAction.CallbackContext ctx)
    {
        Stand?.Invoke();
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        Move?.Invoke(new Vector3(ctx.ReadValue<Vector2>().x, 0f, ctx.ReadValue<Vector2>().y));
    }

    public void OnLaunch()
    {   
        Stand?.Invoke();
        Launch?.Invoke();
    }

    public void OnLand()
    {
        Land?.Invoke();
    }

    public void OnTakeHit(float damage)
    {
        TakeHit?.Invoke(damage);
    }
}
