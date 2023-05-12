using System;
using System.Collections;
using Src.Scripts.Gameplay;
using Src.Scripts.ScriptableObjects;
using Src.Scripts.Utility;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Src.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
        public Player player;
        public Volume postProcessVolume;
        public GameObject gameOverUI;
        public GameObject winUI;
        public GameObject pauseMenu;
        public InputActionProperty pauseButton;
        public GameObject mainMenu;
        public TeamColorScriptableObject teamColorData;
        public UnityEvent onResume; // Used to do things that require the game to be unpaused.
    
        private ColorAdjustments _volumeColorAdjustments;
        private ParentConstraint _menuParentConstraint;
    
        private static readonly int PaintColor1 = Shader.PropertyToID("_PaintColor1");
        private static readonly int PaintColor2 = Shader.PropertyToID("_PaintColor2");
        private static readonly int PaintColor3 = Shader.PropertyToID("_PaintColor3");
        private static readonly int PaintColor4 = Shader.PropertyToID("_PaintColor4");
        private const int PauseSaturationAdjustment = -100;

        private void Start()
        {
            if (player == null)
            {
                player = GameObject.Find("XR Rig").GetComponent<Player>();
            }

            if (gameOverUI == null)
            {
                gameOverUI = GameObject.Find("UI_GameOverMenu");
            }

            if (winUI == null)
            {
                winUI = GameObject.Find("UI_WinMenu");
            }

            if (postProcessVolume != null)
            {
                postProcessVolume.profile.TryGet(out _volumeColorAdjustments);
            }
        
            Initialize();
            SetShaderColors();
        }

        private void Initialize()
        {
            pauseButton.action.performed += OnPauseButtonPressed;
            _volumeColorAdjustments.saturation.value = PauseSaturationAdjustment;
            DisablePauseButton();
            ShowMenu(mainMenu);
            player.DisableGameHands();
            player.EnableUIHands();
            Pause();
        }

        public void PlayerDeath()
        {
            player.DisableOverlayUI();
            player.DisableWeapon();
            player.DisableGameHands();
            player.EnableUIHands();
            Pause();
            ShowGameOverUI();
            _volumeColorAdjustments.saturation.value = PauseSaturationAdjustment;
        }

        public void Win()
        {
            player.DisableOverlayUI();
            player.DisableWeapon();
            player.DisableGameHands();
            player.EnableUIHands();
            Pause();
            ShowWinUI();
            _volumeColorAdjustments.saturation.value = PauseSaturationAdjustment;
        }

        private void OnPauseButtonPressed(InputAction.CallbackContext callbackContext)
        {
            if (Time.timeScale == 0f)
            {
                Unpause();
                _volumeColorAdjustments.saturation.value = 0;
                HideMenu(pauseMenu);
                player.DisableUIHands();
                player.ShowGameHands();
            }
            else
            {
                Pause();
                _volumeColorAdjustments.saturation.value = PauseSaturationAdjustment;
                ShowMenu(pauseMenu);
                player.EnableUIHands();
                player.HideGameHands();
            }
        }

        public void Pause()
        {
            Time.timeScale = 0f;
        }

        public void Unpause()
        { 
            Time.timeScale = 1f;
            onResume.Invoke();
        }

        public void ShowMenu(GameObject menu)
        {
            menu.SetActive(true);
            
            // Stop the menu from moving around while it's open
            if (menu.TryGetComponent(out ParentConstraint parentConstraint))
            {
                StartCoroutine(DisableMenuConstraint(parentConstraint));
            }

            if (menu.TryGetComponent(out RotationConstraint rotationConstraint))
            {
                StartCoroutine(DisableMenuConstraint(rotationConstraint));
            }
        }

        public void HideMenu(GameObject menu)
        {
            menu.SetActive(false);

            if (menu.TryGetComponent(out ParentConstraint parentConstraint))
            {
                parentConstraint.constraintActive = true;
            }

            if (menu.TryGetComponent(out RotationConstraint rotationConstraint))
            {
                rotationConstraint.constraintActive = true;
            }
            
            _volumeColorAdjustments.saturation.value = 0;
        }

        /// <summary>
        /// Deactivates the <paramref name="constraint"/>.
        /// <remarks>This is done in a coroutine because if the constraint's GameObject was disabled,
        /// the constraint must be active for one frame to actually move into position.</remarks>
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        private static IEnumerator DisableMenuConstraint(IConstraint constraint)
        {
            yield return null; // Wait one frame
            constraint.constraintActive = false;
        }
    
        [ContextMenu("Start Game")]
        public void StartGame()
        {
            HideMenu(mainMenu);
            Unpause();
            player.DisableUIHands();
            player.EnableGameHands();
            EnablePauseButton();
        }
    
        private void ShowWinUI()
        {
            winUI.SetActive(true);
            player.DisableGameHands();
            player.EnableUIHands();
        }
    
        private void ShowGameOverUI()
        {
            gameOverUI.SetActive(true);
            gameOverUI.GetComponent<ParentConstraint>().constraintActive = false;
            player.DisableGameHands();
            player.EnableUIHands();
        }
    
        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// Sets the color of each paint channel based on the team color data
        /// </summary>
        public void SetShaderColors()
        {
            Shader.SetGlobalColor(PaintColor1,GetTeamColor(0));
            Shader.SetGlobalColor(PaintColor2,GetTeamColor(1));
            Shader.SetGlobalColor(PaintColor3,GetTeamColor(2));
            Shader.SetGlobalColor(PaintColor4,GetTeamColor(3));
        }
    
        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    
        public Color GetTeamColor(int channel)
        {
            return teamColorData.teamColors[channel];
        }

        public void EnablePauseButton()
        {
            pauseButton.action.Enable();
        }
    
        public void DisablePauseButton()
        {
            pauseButton.action.Disable();
        }
    }
}
