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
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
        public GameObject xrRigGameObject;
        public Volume postProcessVolume;
        public Transform spawnPoint;
        public GameObject gameOverUI;
        public GameObject winUI;
        public GameObject pauseMenu;
        public InputActionProperty pauseButton;
        public GameObject mainMenu;
        public TeamColorScriptableObject teamColorData;
        public UnityEvent onResume;

        private ColorAdjustments _volumeColorAdjustments;
        private ParentConstraint _menuParentConstraint;
        private Player _player;
        private XRRig _xrRig;

        private static readonly int PaintColor1 = Shader.PropertyToID("_PaintColor1");
        private static readonly int PaintColor2 = Shader.PropertyToID("_PaintColor2");
        private static readonly int PaintColor3 = Shader.PropertyToID("_PaintColor3");
        private static readonly int PaintColor4 = Shader.PropertyToID("_PaintColor4");
        private const int PauseSaturationAdjustment = -100;

        private void Start()
        {
            if (xrRigGameObject == null)
            {
                xrRigGameObject = GameObject.Find("XR Rig");
            }

            _xrRig = xrRigGameObject.GetComponent<XRRig>();

            _player = xrRigGameObject.GetComponent<Player>();
            
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
            HideMenu(winUI);
            HideMenu(gameOverUI);
            HideMenu(pauseMenu);
            ShowMenu(mainMenu);
            _player.DisableGameHands();
            _player.EnableUIHands();
            PreferenceManager.Instance.LoadData();
        }

        public void PlayerDeath()
        {
            _player.playerEvents.OnDeath();
            _player.DisableOverlayUI();
            _player.DisableWeapon();
            _player.DisableGameHands();
            _player.EnableUIHands();
            ShowGameOverUI();
            Pause();
        }

        public void RespawnPlayer()
        {
            if (_xrRig == null)
            {
                Debug.Log("GAME_MANAGER: Could not respawn player. XRRig was not found.");
                return;
            }
            
            if (spawnPoint == null)
            {
                Debug.Log("GAME_MANAGER: Could not respawn player. Spawn Point was not found.");
                return;
            }
            
            _xrRig.MoveCameraToWorldLocation(spawnPoint.position);
        }
        public void Win()
        {
            _player.DisableOverlayUI();
            _player.DisableWeapon();
            _player.DisableGameHands();
            _player.EnableUIHands();
            ShowWinUI();
            Pause();
        }

        private void OnPauseButtonPressed(InputAction.CallbackContext callbackContext)
        {
            TogglePause();
        }

        [ContextMenu("Toggle Pause")]
        private void TogglePause()
        {
            if (Time.timeScale == 0f)
            {
                HideMenu(pauseMenu);
                Unpause();
            }
            else
            {
                ShowMenu(pauseMenu);
                Pause();
            }
        }
        
        public void Pause()
        {
            _volumeColorAdjustments.saturation.value = PauseSaturationAdjustment;
            _player.EnableUIHands();
            _player.HideGameHands();
            Time.timeScale = 0f;
        }

        public void Unpause()
        { 
            _volumeColorAdjustments.saturation.value = 0;
            _player.DisableUIHands();
            _player.ShowGameHands();
            Time.timeScale = 1f;
            onResume.Invoke();
        }

        public void ShowMenu(GameObject menu)
        {
            menu.transform.localScale = Vector3.one;

            // Stop the menu from moving around while it's open
            if (menu.TryGetComponent(out ParentConstraint parentConstraint))
            {
                parentConstraint.constraintActive = false;
            }

            if (menu.TryGetComponent(out RotationConstraint rotationConstraint))
            {
                rotationConstraint.constraintActive = false;
            }
        }

        public void HideMenu(GameObject menu)
        {
            menu.transform.localScale = Vector3.zero;
            
            if (menu.TryGetComponent(out ParentConstraint parentConstraint))
            {
                parentConstraint.constraintActive = true;
            }

            if (menu.TryGetComponent(out RotationConstraint rotationConstraint))
            {
                rotationConstraint.constraintActive = true;
            }
        }

        [ContextMenu("Start Game")]
        public void StartGame()
        {
            HideMenu(mainMenu);
            _player.EnableGameHands();
            Unpause();
            EnablePauseButton();
        }
    
        private void ShowWinUI()
        {
            winUI.SetActive(true);
            _player.DisableGameHands();
            _player.EnableUIHands();
        }
    
        private void ShowGameOverUI()
        {
            ShowMenu(gameOverUI);
            _player.DisableGameHands();
            _player.EnableUIHands();
            if (gameOverUI.TryGetComponent(out ParentConstraint constraint))
            {
                constraint.constraintActive = false;
            }
        }
    
        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            Start();
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
