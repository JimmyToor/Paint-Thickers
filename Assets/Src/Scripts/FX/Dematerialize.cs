using System.Runtime.Remoting.Messaging;
using UnityEngine;

namespace FX
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class Dematerialize : MonoBehaviour
    {
        public float timeToDematerialize;
        private bool Dematerializing { get; set; }
        private Material[] _mats;
        private float _bottomPoint;
        private float _topPoint;
        private float _currentHeight;
        private int _heightProperty;
        
        void Start()
        {
            SkinnedMeshRenderer skinnedMeshRend = GetComponent<SkinnedMeshRenderer>();   
            _heightProperty = Shader.PropertyToID("_Height");
            _mats = skinnedMeshRend.materials;
            
            Bounds bounds = skinnedMeshRend.bounds;
            _bottomPoint = transform.InverseTransformPoint(bounds.min).y;
            _topPoint = transform.InverseTransformPoint(bounds.max).y;
            
            foreach (var material in _mats)
            {
                material.SetFloat(_heightProperty, _bottomPoint-1);
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (!Dematerializing)
                return;
        
            foreach (var material in _mats)
            {
                material.SetFloat(_heightProperty, _currentHeight);
            }

            // Effect finished so clean up the object
            if (Mathf.Approximately(_currentHeight, _topPoint)) 
            {
                Destroy(this);
            }
            
            _currentHeight = Mathf.Lerp(_currentHeight, _topPoint, timeToDematerialize);
        }

        public void StartDematerialize()
        {
            _currentHeight = _bottomPoint;
            Dematerializing = true;
        }
    }
}
