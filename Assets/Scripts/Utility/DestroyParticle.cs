using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DestroyParticle : MonoBehaviour
{

    public float lifetime;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject,lifetime);
    }

}
