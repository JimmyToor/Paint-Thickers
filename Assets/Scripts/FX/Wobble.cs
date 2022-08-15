using UnityEngine;

//Code adapted from https://www.patreon.com/posts/18245226 by MinionsArt 
namespace FX
{
    public class Wobble : MonoBehaviour
    {
        private Renderer _rend;
        public Transform driver;
        private Vector3 _lastPos;
        private Vector3 _velocity;
        private Vector3 _lastRot;
        private Vector3 _angularVelocity;
        
        public float wobbleScale = 0.03f;
        public float wobbleSpeed = 1f;
        public float recovery = 1f;
        public float maxWobble = 10f;
        
        private float _wobbleAmountX;
        private float _wobbleAmountZ;
        private float _wobbleAmountToAddX;
        private float _wobbleAmountToAddZ;
        private float _fillToAdd;
        private float _pulse;
        private float _time = 0.5f;
    
        // Use this for initialization
        private void Start()
        {
            _rend = GetComponent<Renderer>();
        }
        private void Update()
        {
            _time += Time.deltaTime;
            // decrease wobble over time
            _wobbleAmountToAddX = Mathf.Lerp(_wobbleAmountToAddX, 0, Time.deltaTime * (recovery));
            _wobbleAmountToAddZ = Mathf.Lerp(_wobbleAmountToAddZ, 0, Time.deltaTime * (recovery));
 
            // make a sine wave of the decreasing wobble
            _pulse = 2 * Mathf.PI * wobbleSpeed;
            _wobbleAmountX = Mathf.Clamp(_wobbleAmountToAddX,-maxWobble,maxWobble) * Mathf.Sin(_pulse * _time);
            _wobbleAmountZ = Mathf.Clamp(_wobbleAmountToAddZ,-maxWobble,maxWobble)* Mathf.Sin(_pulse * _time);
            //rend.material.SetFloat("_Fill", rend.material.GetFloat("_Fill") + wobbleAmountX + wobbleAmountZ);
 
            // send it to the shader
            _rend.material.SetFloat("_WobbleX", _wobbleAmountX);
            _rend.material.SetFloat("_WobbleZ", _wobbleAmountZ);
 
            // velocity
            _velocity = (_lastPos - driver.position) / Time.deltaTime;
            _angularVelocity = driver.rotation.eulerAngles - _lastRot;
 
 
            // add clamped velocity to wobble
            _wobbleAmountToAddX += Mathf.Clamp((_velocity.x) * wobbleScale, -wobbleScale, wobbleScale);
            _wobbleAmountToAddZ += Mathf.Clamp((_velocity.z) * wobbleScale, -wobbleScale, wobbleScale);
 
            // keep last position
            _lastPos = driver.position;
            _lastRot = driver.rotation.eulerAngles;
        }
 
 
 
    }
}