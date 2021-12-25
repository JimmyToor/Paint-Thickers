using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Code adapted from https://www.patreon.com/posts/18245226 by MinionsArt 
public class Wobble : MonoBehaviour
{
    Renderer rend;
    public Transform driver;
    Vector3 lastPos;
    Vector3 velocity;
    Vector3 lastRot;  
    Vector3 angularVelocity;
    public float WobbleScale = 0.03f;
    public float WobbleSpeed = 1f;
    public float Recovery = 1f;
    public float MaxWobble = 10f;
    float wobbleAmountX;
    float wobbleAmountZ;
    float wobbleAmountToAddX;
    float wobbleAmountToAddZ;
    float fillToAdd;
    float pulse;
    float time = 0.5f;
    
    // Use this for initialization
    void Start()
    {
        rend = GetComponent<Renderer>();
    }
    private void Update()
    {
        time += Time.deltaTime;
        // decrease wobble over time
        wobbleAmountToAddX = Mathf.Lerp(wobbleAmountToAddX, 0, Time.deltaTime * (Recovery));
        wobbleAmountToAddZ = Mathf.Lerp(wobbleAmountToAddZ, 0, Time.deltaTime * (Recovery));
 
        // make a sine wave of the decreasing wobble
        pulse = 2 * Mathf.PI * WobbleSpeed;
        wobbleAmountX = Mathf.Clamp(wobbleAmountToAddX,-MaxWobble,MaxWobble) * Mathf.Sin(pulse * time);
        wobbleAmountZ = Mathf.Clamp(wobbleAmountToAddZ,-MaxWobble,MaxWobble)* Mathf.Sin(pulse * time);
        //rend.material.SetFloat("_Fill", rend.material.GetFloat("_Fill") + wobbleAmountX + wobbleAmountZ);
 
        // send it to the shader
        rend.material.SetFloat("_WobbleX", wobbleAmountX);
        rend.material.SetFloat("_WobbleZ", wobbleAmountZ);
 
        // velocity
        velocity = (lastPos - driver.position) / Time.deltaTime;
        angularVelocity = driver.rotation.eulerAngles - lastRot;
 
 
        // add clamped velocity to wobble
        wobbleAmountToAddX += Mathf.Clamp((velocity.x) * WobbleScale, -WobbleScale, WobbleScale);
        wobbleAmountToAddZ += Mathf.Clamp((velocity.z) * WobbleScale, -WobbleScale, WobbleScale);
 
        // keep last position
        lastPos = driver.position;
        lastRot = driver.rotation.eulerAngles;
    }
 
 
 
}