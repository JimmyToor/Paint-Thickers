using System;
using System.ComponentModel;
using Src.Scripts.Gameplay;
using Src.Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Preferences
{
    /// <summary>
    /// Acts as a central manager to affect changes to user preferences.
    /// </summary>
    public class UserPreferencesManager : MonoBehaviour
    {
        public enum TurnStyle
        {
            Snap,
            Smooth,
        }

        public enum MainHand
        {
            Left,
            Right,
        }

        public enum VignetteStrength
        {
            Off,
            Low,
            Med,
            High,
        }

        public enum MovementOrientation
        {
            Head,
            OffHand,
        }
        
        private const float LowVignetteValue = 0.2f;
        private const float MedVignetteValue = 0.5f;
        private const float HighVignetteValue = 0.8f;

        private InputActionMap _leftHandActionMap;
        private InputActionMap _rightHandActionMap;
        private InputControlScheme? _baseControlScheme;
        private InputControlScheme? _rightHandedControlScheme;
        private InputControlScheme? _leftHandedControlScheme;

        [SerializeField]
        private TurnStyle turningStyle;
        public TurnStyle TurningStyle
        {
            get => turningStyle;
            set
            {
                SetTurnStyle(value);
                turningStyle = value;
            }
        }
        
        [SerializeField]
        private MainHand preferredHand;
        public MainHand PreferredHand
        {
            get => preferredHand;
            set
            {
                SetMainHand(value);
                preferredHand = value;
            }
        }
        
        [SerializeField]
        private MovementOrientation forwardReference;
        public MovementOrientation ForwardReference
        {
            get => forwardReference;
            set
            {
                SetForwardReference(value);
                forwardReference = value;
            }
        }

        [SerializeField] 
        private VignetteStrength vignetteIntensity;
        public VignetteStrength VignetteIntensity
        {
            get => vignetteIntensity;
            set
            {
                SetVignetteStrength(value);
                vignetteIntensity = value;
            }
        }

        [SerializeField]
        private ActionBasedContinuousMoveProvider smoothMoveProvider;
        public ActionBasedContinuousMoveProvider SmoothMoveProvider
        {
            get => smoothMoveProvider;
            set => smoothMoveProvider = value;
        }
        
        [SerializeField]
        private ActionBasedContinuousTurnProvider smoothTurnProvider;
        public ActionBasedContinuousTurnProvider SmoothTurnProvider
        {
            get => smoothTurnProvider;
            set => smoothTurnProvider = value;
        }
        
        [SerializeField]
        private ActionBasedSnapTurnProvider snapTurnProvider;
        public ActionBasedSnapTurnProvider SnapTurnProvider
        {
            get => snapTurnProvider;
            set => snapTurnProvider = value;
        }
        
        [SerializeField] 
        private ComfortVignette comfortVignette;
        public ComfortVignette ComfortVignette
        {
            get => comfortVignette;
            set => comfortVignette = value;
        }

        [SerializeField]
        private Player player;
        public Player Player
        {
            get => player;
            set => player = value;
        }

        [SerializeField]
        private InputActionAsset inputs;
        public InputActionAsset Inputs
        {
            get => inputs;
            set => inputs = value;
        }

        [SerializeField]
        private string baseControlSchemeName;
        public string BaseControlSchemeName
        {
            get => baseControlSchemeName;
            set => baseControlSchemeName = value;
        }

        [SerializeField]
        private string leftHandedControlSchemeName;
        public string LeftHandedControlSchemeName
        {
            get => leftHandedControlSchemeName;
            set => leftHandedControlSchemeName = value;
        }
        
        [SerializeField]
        private string rightHandedControlSchemeName;
        public string RightHandedControlSchemeName
        {
            get => rightHandedControlSchemeName;
            set => rightHandedControlSchemeName = value;
        }
        
        private void Awake()
        {
            GetInputs();
        }

        private void GetInputs()
        {
            _leftHandActionMap = inputs.FindActionMap("XRI LeftHand");
            _rightHandActionMap = inputs.FindActionMap("XRI RightHand");
            
            _leftHandedControlScheme = FindControlScheme(leftHandedControlSchemeName);
            _rightHandedControlScheme = FindControlScheme(rightHandedControlSchemeName);
            _baseControlScheme = FindControlScheme(baseControlSchemeName);
            
        }

        void Start()
        {
            SetTurnStyle(TurningStyle);
            SetMainHand(PreferredHand);
            SetVignetteStrength(VignetteIntensity);
        }

        void SetTurnStyle(TurnStyle style)
        {
            switch (style)
            {
                case TurnStyle.Snap:
                    if (SmoothTurnProvider != null)
                    {
                        SmoothTurnProvider.enabled = false;
                    }

                    if (SnapTurnProvider != null)
                    {
                        // If the Continuous Turn and Snap Turn providers both use the same
                        // action, then disabling the first provider will cause the action to be
                        // disabled, so the action needs to be enabled, which is done by forcing
                        // the OnEnable() of the second provider to be called.
                        // ReSharper disable Unity.InefficientPropertyAccess
                        SnapTurnProvider.enabled = false;
                        SnapTurnProvider.enabled = true;
                        // ReSharper restore Unity.InefficientPropertyAccess
                        SnapTurnProvider.enableTurnLeftRight = true;
                    }
                    break;
                case TurnStyle.Smooth:
                    if (SnapTurnProvider != null)
                    {
                        SnapTurnProvider.enabled = false;
                    }

                    if (SmoothTurnProvider != null)
                    {
                        SmoothTurnProvider.enabled = false;
                        // ReSharper disable once Unity.InefficientPropertyAccess
                        SmoothTurnProvider.enabled = true;
                    }
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(TurnStyle));
            }
        }

        void SetMainHand(MainHand hand)
        {
            switch (hand)
            {
                case MainHand.Left:
                    SetHandBindingMasks(_leftHandedControlScheme);
                    GameManager.Instance.onResume.AddListener(()=> Player.MainHand = MainHand.Left);
                    break;
                
                case MainHand.Right:
                    SetHandBindingMasks(_rightHandedControlScheme);
                    GameManager.Instance.onResume.AddListener(()=> Player.MainHand = MainHand.Right);
                    break;
                
                default:
                    throw new InvalidEnumArgumentException(nameof(hand), (int)hand, typeof(MainHand));
            }
            
            // Set the forward reference again since the offhand might have changed
            if (forwardReference == MovementOrientation.OffHand)
            {
                SetForwardReference(MovementOrientation.OffHand);
            }
        }

        void SetForwardReference(MovementOrientation forwardRef)
        {
            switch (forwardRef)
            {
                case MovementOrientation.Head:
                    smoothMoveProvider.forwardSource = Camera.main.transform;
                    break;
                case MovementOrientation.OffHand:
                    smoothMoveProvider.forwardSource = preferredHand == MainHand.Left ? player.rightHand.transform : player.leftHand.transform;
                    break;
                
                default:
                    throw new InvalidEnumArgumentException(nameof(forwardRef), (int)forwardRef, typeof(MovementOrientation));
            }
        }

        private void SetHandBindingMasks(InputControlScheme? controlScheme)
        {
            _leftHandActionMap.bindingMask = InputBinding.MaskByGroups(_baseControlScheme?.bindingGroup, controlScheme?.bindingGroup);
            _rightHandActionMap.bindingMask = InputBinding.MaskByGroups(_baseControlScheme?.bindingGroup, controlScheme?.bindingGroup);
        }
        
        InputControlScheme? FindControlScheme(string controlSchemeName)
        {
            if (string.IsNullOrEmpty(controlSchemeName))
                return null;

            var scheme = inputs.FindControlScheme(controlSchemeName);
            if (scheme == null)
            {
                Debug.LogError($"Cannot find control scheme \"{controlSchemeName}\" in '{inputs}'.", this);
                return null;
            }

            return scheme;
        }

        void SetVignetteStrength(VignetteStrength strength)
        {
            switch (strength)
            {
                case VignetteStrength.Off:
                    ComfortVignette.intensity = 0f;
                    break;
                case VignetteStrength.Low:
                    ComfortVignette.intensity = LowVignetteValue;
                    break;
                case VignetteStrength.Med:
                    ComfortVignette.intensity = MedVignetteValue;
                    break;
                case VignetteStrength.High:
                    ComfortVignette.intensity = HighVignetteValue;
                    break;
                default:
                    Debug.LogError("USERPREFERENCES: Invalid vignette value: " + strength);
                    break;
            }
        }
    }
}
