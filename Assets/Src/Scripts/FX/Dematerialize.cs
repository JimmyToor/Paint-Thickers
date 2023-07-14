using System.Collections;
using UnityEngine;

namespace Src.Scripts.FX
{
    [RequireComponent(typeof(SkinnedMeshRenderer))]
    public class Dematerialize : MonoBehaviour
    {
        public float timeToDematerialize;
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
            
            Bounds bounds = skinnedMeshRend.localBounds;
            _bottomPoint = bounds.min.y;
            _topPoint = bounds.max.y+1;
            
            foreach (var material in _mats)
            {
                material.SetFloat(_heightProperty, _bottomPoint);
            }
        }

        // Update is called once per frame
        IEnumerator DematerializeOverTime()
        {
            float maxDelta = _topPoint - _currentHeight;
            while (_currentHeight < _topPoint)
            {
                foreach (var material in _mats)
                {
                    material.SetFloat(_heightProperty, _currentHeight);
                }
                _currentHeight = Mathf.MoveTowards(_currentHeight, _topPoint, (maxDelta*timeToDematerialize) * Time.deltaTime);
                yield return null;
            }
            Destroy(gameObject);
        }

        [ContextMenu("Start Dematerialize")]
        public void StartDematerialize()
        {
            _currentHeight = _bottomPoint;
            StartCoroutine(DematerializeOverTime());
        }
    }
}
