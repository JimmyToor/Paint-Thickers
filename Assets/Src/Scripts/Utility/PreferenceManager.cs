using System;
using Src.Scripts.Preferences;
using UnityEngine;

namespace Src.Scripts.Utility
{
    public class PreferenceManager : Singleton<PreferenceManager>
    {
        public UserPreferencesManager userPreferencesManager;

        private void OnEnable()
        {
            GameManager.Instance.onResume.AddListener(SaveData);
        }

        private void OnDisable()
        {
            GameManager.Instance.onResume.RemoveListener(SaveData);
        }

        private void Start()
        {
            if (userPreferencesManager == null)
            {
                userPreferencesManager = FindObjectOfType<UserPreferencesManager>();
            }
        }
        
        public void SaveData()
        {
            PlayerPrefs.SetInt("TurnStyle", (int)userPreferencesManager.TurningStyle);

            PlayerPrefs.SetInt("PreferredHand", (int)userPreferencesManager.PreferredHand);

            PlayerPrefs.SetInt("ForwardReference",(int)userPreferencesManager.ForwardReference);

            PlayerPrefs.SetInt("VignetteIntensity", (int)userPreferencesManager.VignetteIntensity);

            PlayerPrefs.SetString("BaseControlSchemeName", userPreferencesManager.BaseControlSchemeName);

            PlayerPrefs.SetString("LeftHandedControlSchemeName", userPreferencesManager.LeftHandedControlSchemeName);

            PlayerPrefs.SetString("RightHandedControlSchemeName", userPreferencesManager.RightHandedControlSchemeName);

            PlayerPrefs.Save();
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
        }
    }
}