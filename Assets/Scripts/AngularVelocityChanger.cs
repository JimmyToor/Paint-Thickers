using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AngularVelocityChanger : MonoBehaviour
{
    [SerializeField]
    float NewMaxAngularVelocity = 20f;
    const float defaultMaxAngularVelocity = 7f;

    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    public void SetNewMaxAngularVelocity()
    {
        rb.maxAngularVelocity = NewMaxAngularVelocity;
    }

    public void ResetMaxAngularVelocity()
    {
        rb.maxAngularVelocity = defaultMaxAngularVelocity;
    }
    
}
