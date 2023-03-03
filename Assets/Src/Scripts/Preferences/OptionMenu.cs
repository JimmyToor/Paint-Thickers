using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Scripts.Preferences
{
    public class OptionMenu : MonoBehaviour
    {
        public UserPreferences manager;
        public Toggle leftHandToggle;
        public Toggle rightHandToggle;
        public Toggle vignetteOff;
        public Toggle vignetteLow;
        public Toggle vignetteMed;
        public Toggle vignetteHigh;
        public Toggle smoothTurnToggle;
        public Toggle snapTurnToggle;
        public Slider smoothTurnSpeedSlider;
        public TextMeshProUGUI smoothTurnSpeedText;
        public Slider snapTurnIncrementSlider;
        public TextMeshProUGUI snapTurnAmountText;

        private void Awake()
        {
            leftHandToggle.onValueChanged.AddListener(OnLeftHandToggled);
            rightHandToggle.onValueChanged.AddListener(OnRightHandToggled);
            snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggled);
            smoothTurnToggle.onValueChanged.AddListener(OnSmoothTurnToggled);
            snapTurnIncrementSlider.onValueChanged.AddListener(ChangeSnapTurnAmount);
            smoothTurnSpeedSlider.onValueChanged.AddListener(ChangeSmoothTurnSpeed);
            vignetteOff.onValueChanged.AddListener(VignetteOffToggled);
            vignetteLow.onValueChanged.AddListener(VignetteLowToggled);
            vignetteMed.onValueChanged.AddListener(VignetteMedToggled);
            vignetteHigh.onValueChanged.AddListener(VignetteHighToggled);
        }

        private void Start()
        {
            InitializeValues();
        }

        private void OnLeftHandToggled(bool value)
        {
            manager.PreferredHand = UserPreferences.MainHand.Left;
        }
        private void OnRightHandToggled(bool value)
        {
            manager.PreferredHand = UserPreferences.MainHand.Right;
        }

        private void VignetteOffToggled(bool value)
        {
            if (value)
            {
                manager.VignetteIntensity = UserPreferences.VignetteStrength.Off;
            }
        }
        private void VignetteLowToggled(bool value)
        {
            if (value)
            {
                manager.VignetteIntensity = UserPreferences.VignetteStrength.Low;
            }
        }
        private void VignetteMedToggled(bool value)
        {
            if (value)
            {
                manager.VignetteIntensity = UserPreferences.VignetteStrength.Med;
            }
        }
        private void VignetteHighToggled(bool value)
        {
            if (value)
            {
                manager.VignetteIntensity = UserPreferences.VignetteStrength.High;
            }
        }

        private void InitializeValues()
        {
            if (manager.Player == null)
            {
                manager.Player = GameObject.Find("XR Rig").GetComponent<Player>();
            }

            manager.Player.WeaponHand = manager.PreferredHand;
            
            leftHandToggle.isOn = manager.PreferredHand == UserPreferences.MainHand.Left;
            rightHandToggle.isOn = manager.PreferredHand == UserPreferences.MainHand.Right;
            
            smoothTurnSpeedSlider.value = manager.SmoothTurnProvider.turnSpeed;
            smoothTurnToggle.isOn = manager.TurningStyle == UserPreferences.TurnStyle.Smooth;
            smoothTurnSpeedText.text = smoothTurnSpeedSlider.value.ToString();

            snapTurnIncrementSlider.value = manager.SnapTurnProvider.turnAmount;
            snapTurnToggle.isOn = manager.TurningStyle == UserPreferences.TurnStyle.Snap;
            snapTurnAmountText.text = snapTurnIncrementSlider.value.ToString();
            
            smoothTurnToggle.isOn = manager.TurningStyle == UserPreferences.TurnStyle.Smooth;
            snapTurnToggle.isOn = manager.TurningStyle == UserPreferences.TurnStyle.Snap;

            switch (manager.VignetteIntensity)
            {
                case UserPreferences.VignetteStrength.Off:
                    vignetteOff.isOn = true;
                    break;
                case UserPreferences.VignetteStrength.Low:
                    vignetteLow.isOn = true;
                    break;
                case UserPreferences.VignetteStrength.Med:
                    vignetteMed.isOn = true;
                    break;
                case UserPreferences.VignetteStrength.High:
                    vignetteHigh.isOn = true;
                    break;
                default:
                    throw new NotSupportedException("Invalid input update mode: " + manager.VignetteIntensity);
            }
        }
    
        private void ChangeSmoothTurnSpeed(float value)
        {
            manager.SmoothTurnProvider.turnSpeed = value;
            smoothTurnSpeedText.text = value.ToString();
        }
    
        private void ChangeSnapTurnAmount(float value)
        {
            manager.SmoothTurnProvider.turnSpeed = value;
            snapTurnAmountText.text = value.ToString();
        }

        private void OnSnapTurnToggled(bool value)
        {
            if (value)
            {
                manager.TurningStyle = UserPreferences.TurnStyle.Snap;
            }
        }
    
        private void OnSmoothTurnToggled(bool value)
        {
            if (value)
            {
                manager.TurningStyle = UserPreferences.TurnStyle.Smooth;
            }
        }
    }
}
