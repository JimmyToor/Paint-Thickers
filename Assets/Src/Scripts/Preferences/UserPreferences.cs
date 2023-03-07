using System;
using System.ComponentModel;
using UI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Preferences
{
    /// <summary>
    /// Acts as a central manager to affect changes to user preferences.
    /// </summary>
    public class UserPreferences : MonoBehaviour
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
        
        private const float LowVignetteValue = 0.2f;
        private const float MedVignetteValue = 0.5f;
        private const float HighVignetteValue = 0.8f;

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
        ActionBasedContinuousMoveProvider smoothMoveProvider;
        public ActionBasedContinuousMoveProvider SmoothMoveProvider
        {
            get => smoothMoveProvider;
            set => smoothMoveProvider = value;
        }
        
        [SerializeField]
        ActionBasedContinuousTurnProvider smoothTurnProvider;
        public ActionBasedContinuousTurnProvider SmoothTurnProvider
        {
            get => smoothTurnProvider;
            set => smoothTurnProvider = value;
        }
        
        [SerializeField]
        ActionBasedSnapTurnProvider snapTurnProvider;
        public ActionBasedSnapTurnProvider SnapTurnProvider
        {
            get => snapTurnProvider;
            set => snapTurnProvider = value;
        }
        
        [SerializeField] 
        ComfortVignette comfortVignette;
        public ComfortVignette ComfortVignette
        {
            get => comfortVignette;
            set => comfortVignette = value;
        }

        [SerializeField]
        Player player;
        public Player Player
        {
            get => player;
            set => player = value;
        }
        

        void Awake()
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
                    if (SmoothTurnProvider != null)
                    {
                        SmoothTurnProvider.leftHandTurnAction.action.Enable();
                        SmoothTurnProvider.rightHandTurnAction.action.Disable();
                    }

                    if (SnapTurnProvider != null)
                    {
                        SnapTurnProvider.leftHandSnapTurnAction.action.Enable();
                        SnapTurnProvider.rightHandSnapTurnAction.action.Disable();
                    }

                    if (SmoothMoveProvider != null)
                    {
                        SmoothMoveProvider.rightHandMoveAction.action.Enable();
                        SmoothMoveProvider.leftHandMoveAction.action.Disable();
                    }
                    
                    Player.WeaponHand = MainHand.Left;
                    break;
                case MainHand.Right:
                    if (SnapTurnProvider != null)
                    {
                        SmoothTurnProvider.rightHandTurnAction.action.Enable();
                        SmoothTurnProvider.leftHandTurnAction.action.Disable();
                    }

                    if (SmoothTurnProvider != null)
                    {
                        SnapTurnProvider.rightHandSnapTurnAction.action.Enable();
                        SnapTurnProvider.leftHandSnapTurnAction.action.Disable();
                    }
                    
                    if (SmoothMoveProvider != null)
                    {
                        SmoothMoveProvider.leftHandMoveAction.action.Enable();
                        SmoothMoveProvider.rightHandMoveAction.action.Disable();
                    }
                    
                    Player.WeaponHand = MainHand.Right;
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(hand), (int)hand, typeof(MainHand));
            }
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
                    throw new NotSupportedException("Invalid input update mode: " + VignetteIntensity);
            }
        }
    }
}
