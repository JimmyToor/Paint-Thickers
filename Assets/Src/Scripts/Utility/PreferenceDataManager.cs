using Src.Scripts.Preferences;
using UnityEngine;

namespace Src.Scripts.Utility
{
    /// <summary>
    /// Handles saving and loading player preferences
    /// </summary>
    public class PreferenceDataManager : Singleton<PreferenceDataManager>
    {
        public UserPreferencesManager userPreferencesManager;
        public PauseHandler pauseHandler;

        [SerializeField] private bool isDirty = false;
        
        private void OnEnable()
        {
            pauseHandler.onResume.AddListener(SaveData);
        }

        private void OnDisable()
        {
            pauseHandler.onResume.RemoveListener(SaveData);
        }

        public override void Awake()
        {
            base.Awake();
            if (userPreferencesManager == null)
            {
                userPreferencesManager = FindObjectOfType<UserPreferencesManager>();
            }
        }
        
        public void SaveData()
        {
            if (!isDirty) return;
            
            PlayerPrefs.SetInt("TurnStyle", (int)userPreferencesManager.TurningStyle);

            PlayerPrefs.SetInt("PreferredHand", (int)userPreferencesManager.PreferredHand);

            PlayerPrefs.SetInt("ForwardReference",(int)userPreferencesManager.ForwardReference);

            PlayerPrefs.SetInt("VignetteIntensity", (int)userPreferencesManager.VignetteIntensity);
            
            PlayerPrefs.SetFloat("SmoothTurnSpeed", (int)userPreferencesManager.SmoothTurnSpeed);
            
            PlayerPrefs.SetFloat("SnapTurnAmount", (int)userPreferencesManager.SnapTurnAmount);

            PlayerPrefs.SetString("BaseControlSchemeName", userPreferencesManager.BaseControlSchemeName);

            PlayerPrefs.SetString("LeftHandedControlSchemeName", userPreferencesManager.LeftHandedControlSchemeName);

            PlayerPrefs.SetString("RightHandedControlSchemeName", userPreferencesManager.RightHandedControlSchemeName);

            PlayerPrefs.Save();
            
            MarkClean();
        }

        public void LoadData()
        {
            if (PlayerPrefs.HasKey("BaseControlSchemeName"))
                userPreferencesManager.BaseControlSchemeName = PlayerPrefs.GetString("BaseControlSchemeName");

            if (PlayerPrefs.HasKey("LeftHandedControlSchemeName"))
                userPreferencesManager.LeftHandedControlSchemeName = PlayerPrefs.GetString("LeftHandedControlSchemeName");

            if (PlayerPrefs.HasKey("RightHandedControlSchemeName"))
                userPreferencesManager.RightHandedControlSchemeName = PlayerPrefs.GetString("RightHandedControlSchemeName");
            
            if (PlayerPrefs.HasKey("TurnStyle"))
                userPreferencesManager.TurningStyle = (UserPreferencesManager.TurnStyle)PlayerPrefs.GetInt("TurnStyle");

            if (PlayerPrefs.HasKey("PreferredHand"))
                userPreferencesManager.PreferredHand = (UserPreferencesManager.MainHand)PlayerPrefs.GetInt("PreferredHand");

            if (PlayerPrefs.HasKey("ForwardReference"))
                userPreferencesManager.ForwardReference = (UserPreferencesManager.MovementOrientation)PlayerPrefs.GetInt("ForwardReference");

            if (PlayerPrefs.HasKey("VignetteIntensity"))
                userPreferencesManager.VignetteIntensity = (UserPreferencesManager.VignetteStrength)PlayerPrefs.GetInt("VignetteIntensity");
            
            if (PlayerPrefs.HasKey("SmoothTurnSpeed"))
                userPreferencesManager.SmoothTurnSpeed = PlayerPrefs.GetFloat("SmoothTurnSpeed");
            
            if (PlayerPrefs.HasKey("SnapTurnAmount"))
                userPreferencesManager.SnapTurnAmount = PlayerPrefs.GetFloat("SnapTurnAmount");
        }

        public void MarkDirty() => isDirty = true;

        private void MarkClean() => isDirty = false;
    }
}