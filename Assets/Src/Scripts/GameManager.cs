using System.Collections;
using ScriptableObjects;
using Src.Scripts.Gameplay;
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
            DisablePause();
            ShowMenu(mainMenu);
            Pause();
        }

        public void PlayerDeath()
        {
            player.DisableOverlayUI();
            player.DisableWeapon();
            player.ToggleUIRays();
            ShowGameOverUI();
        }

        public void Win()
        {
            ShowWinUI();
        }

        private void OnPauseButtonPressed(InputAction.CallbackContext callbackContext)
        {
            if (Time.timeScale == 0f)
            {
                Unpause();
                _volumeColorAdjustments.saturation.value = 0;
                HideMenu(pauseMenu);
            }
            else
            {
                Pause();
                _volumeColorAdjustments.saturation.value = PauseSaturationAdjustment;
                ShowMenu(pauseMenu);
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
            menu.TryGetComponent(out ParentConstraint parentConstraint);
            menu.TryGetComponent(out RotationConstraint rotationConstraint);
            
            StartCoroutine(DisableMenuConstraint(parentConstraint));
            StartCoroutine(DisableMenuConstraint(rotationConstraint));

            player.EnableUIHands();
        }

        public void HideMenu(GameObject menu)
        {
            menu.SetActive(false);
            menu.TryGetComponent(out ParentConstraint parentConstraint);
            menu.TryGetComponent(out RotationConstraint rotationConstraint);

            parentConstraint.constraintActive = true;
            rotationConstraint.constraintActive = true;

            _volumeColorAdjustments.saturation.value = 0;
            player.EnableGameHands();
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
            player.ToggleUIRays();
            player.EnableHands();
            EnablePause();
        }
    
        private void ShowWinUI()
        {
            winUI.SetActive(true);
            player.EnableUIHands();
        }
    
        private void ShowGameOverUI()
        {
            gameOverUI.SetActive(true);
            gameOverUI.GetComponent<ParentConstraint>().constraintActive = false;
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

        public void EnablePause()
        {
            pauseButton.action.Enable();
        }
    
        public void DisablePause()
        {
            pauseButton.action.Disable();
        }
    }
}
