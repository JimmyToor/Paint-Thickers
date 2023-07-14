using System;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class AdjustSlider : MonoBehaviour
    {
        public float value;
        public Slider slider;

        public void AdjustSliderValue()
        {
            slider.value += value;
        }
    
    }
}
