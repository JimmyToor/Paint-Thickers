using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Scripts.Preferences
{
    public class OptionMenu : MonoBehaviour
    {
        public UserPreferencesManager userPreferencesManager;
        public Toggle leftHandToggle;
        public Toggle rightHandToggle;
        public Toggle headToggle;
        public Toggle offHandToggle;
        public Toggle vignetteOff;
        public Toggle vignetteLow;
        public Toggle vignetteMed;
        public Toggle vignetteHigh;
        public Toggle smoothTurnToggle;
        public Toggle snapTurnToggle;
        public Slider smoothTurnSpeedSlider;
        public TextMeshProUGUI smoothTurnSpeedText;
        public float smoothTurnIncrements;
        public Slider snapTurnIncrementSlider;
        public TextMeshProUGUI snapTurnAmountText;
        public float snapTurnIncrements;
        
        
        private void Awake()
        {
            if (userPreferencesManager == null)
            {
                Debug.LogError("Manager missing from option menu!");
            }
            InitializeValues();
        }

        private void OnEnable()
        {
            leftHandToggle.onValueChanged.AddListener(OnLeftHandToggled);
            rightHandToggle.onValueChanged.AddListener(OnRightHandToggled);
            headToggle.onValueChanged.AddListener(OnHeadToggled);
            offHandToggle.onValueChanged.AddListener(OnOffHandToggled);
            snapTurnToggle.onValueChanged.AddListener(OnSnapTurnToggled);
            smoothTurnToggle.onValueChanged.AddListener(OnSmoothTurnToggled);
            snapTurnIncrementSlider.onValueChanged.AddListener(ChangeSnapTurnAmount);
            smoothTurnSpeedSlider.onValueChanged.AddListener(ChangeSmoothTurnSpeed);
            vignetteOff.onValueChanged.AddListener(VignetteOffToggled);
            vignetteLow.onValueChanged.AddListener(VignetteLowToggled);
            vignetteMed.onValueChanged.AddListener(VignetteMedToggled);
            vignetteHigh.onValueChanged.AddListener(VignetteHighToggled);
        }

        private void OnDisable()
        {
            leftHandToggle.onValueChanged.RemoveListener(OnLeftHandToggled);
            rightHandToggle.onValueChanged.RemoveListener(OnRightHandToggled);
            snapTurnToggle.onValueChanged.RemoveListener(OnSnapTurnToggled);
            smoothTurnToggle.onValueChanged.RemoveListener(OnSmoothTurnToggled);
            snapTurnIncrementSlider.onValueChanged.RemoveListener(ChangeSnapTurnAmount);
            smoothTurnSpeedSlider.onValueChanged.RemoveListener(ChangeSmoothTurnSpeed);
            vignetteOff.onValueChanged.RemoveListener(VignetteOffToggled);
            vignetteLow.onValueChanged.RemoveListener(VignetteLowToggled);
            vignetteMed.onValueChanged.RemoveListener(VignetteMedToggled);
            vignetteHigh.onValueChanged.RemoveListener(VignetteHighToggled);
        }

        private void OnLeftHandToggled(bool value)
        {
            userPreferencesManager.PreferredHand = UserPreferencesManager.MainHand.Left;
        }
        
        private void OnRightHandToggled(bool value)
        {
            userPreferencesManager.PreferredHand = UserPreferencesManager.MainHand.Right;
        }
        
        private void OnHeadToggled(bool value)
        {
            userPreferencesManager.ForwardReference = UserPreferencesManager.MovementOrientation.Head;
        }
        
        private void OnOffHandToggled(bool value)
        {
            userPreferencesManager.ForwardReference = UserPreferencesManager.MovementOrientation.OffHand;
        }

        private void VignetteOffToggled(bool value)
        {
            if (value)
            {
                userPreferencesManager.VignetteIntensity = UserPreferencesManager.VignetteStrength.Off;
            }
        }
        private void VignetteLowToggled(bool value)
        {
            if (value)
            {
                userPreferencesManager.VignetteIntensity = UserPreferencesManager.VignetteStrength.Low;
            }
        }
        private void VignetteMedToggled(bool value)
        {
            if (value)
            {
                userPreferencesManager.VignetteIntensity = UserPreferencesManager.VignetteStrength.Med;
            }
        }
        private void VignetteHighToggled(bool value)
        {
            if (value)
            {
                userPreferencesManager.VignetteIntensity = UserPreferencesManager.VignetteStrength.High;
            }
        }

        private void InitializeValues()
        {
            leftHandToggle.isOn = userPreferencesManager.PreferredHand == UserPreferencesManager.MainHand.Left;
            rightHandToggle.isOn = userPreferencesManager.PreferredHand == UserPreferencesManager.MainHand.Right;

            smoothTurnSpeedSlider.value = userPreferencesManager.SmoothTurnProvider.turnSpeed/smoothTurnIncrements;
            smoothTurnToggle.isOn = userPreferencesManager.TurningStyle == UserPreferencesManager.TurnStyle.Smooth;
            ChangeSmoothTurnSpeed(smoothTurnSpeedSlider.value);

            snapTurnIncrementSlider.value = userPreferencesManager.SnapTurnProvider.turnAmount/snapTurnIncrements;
            snapTurnToggle.isOn = userPreferencesManager.TurningStyle == UserPreferencesManager.TurnStyle.Snap;
            ChangeSnapTurnAmount(snapTurnIncrementSlider.value);
            
            smoothTurnToggle.isOn = userPreferencesManager.TurningStyle == UserPreferencesManager.TurnStyle.Smooth;
            snapTurnToggle.isOn = userPreferencesManager.TurningStyle == UserPreferencesManager.TurnStyle.Snap;
            switch (userPreferencesManager.VignetteIntensity)
            {
                case UserPreferencesManager.VignetteStrength.Off:
                    vignetteOff.isOn = true;
                    break;
                case UserPreferencesManager.VignetteStrength.Low:
                    vignetteLow.isOn = true;
                    break;
                case UserPreferencesManager.VignetteStrength.Med:
                    vignetteMed.isOn = true;
                    break;
                case UserPreferencesManager.VignetteStrength.High:
                    vignetteHigh.isOn = true;
                    break;
                default:
                    Debug.LogError("OPTIONSMENU: Invalid vignette setting: " + userPreferencesManager.VignetteIntensity);
                    break;
            }
        }
    
        private void ChangeSmoothTurnSpeed(float value)
        {
            float speed = value * smoothTurnIncrements;
            userPreferencesManager.SmoothTurnProvider.turnSpeed = speed;
            smoothTurnSpeedText.text = speed.ToString();
        }
    
        private void ChangeSnapTurnAmount(float value)
        {
            float speed = value * snapTurnIncrements;
            userPreferencesManager.SmoothTurnProvider.turnSpeed = speed;
            snapTurnAmountText.text = speed.ToString();
        }

        private void OnSnapTurnToggled(bool value)
        {
            if (value)
            {
                userPreferencesManager.TurningStyle = UserPreferencesManager.TurnStyle.Snap;
            }
        }
    
        private void OnSmoothTurnToggled(bool value)
        {
            if (value)
            {
                userPreferencesManager.TurningStyle = UserPreferencesManager.TurnStyle.Smooth;
            }
        }
    }
}
