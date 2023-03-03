using System.Collections;
using System.Collections.Generic;
using ScriptableObjects;
using Src.Scripts;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

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
        pauseButton.action.Disable();
        ShowStartMenu();
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
            HidePauseMenu();
        }
        else
        {
            Pause();
            _volumeColorAdjustments.saturation.value = PauseSaturationAdjustment;
            ShowPauseMenu();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        Time.timeScale = 1f;
    }
    
    public void ShowPauseMenu()
    {
        pauseMenu.gameObject.SetActive(true);
        pauseMenu.TryGetComponent(out ParentConstraint menuConstraint);
        
        // Stop the menu from moving around while it's open
        if (menuConstraint != null)
        {
            StartCoroutine(DisableMenuConstraint(menuConstraint));
        }

        player.EnableUIHands();
    }

    public void HidePauseMenu()
    {
        pauseMenu.gameObject.SetActive(false);
        pauseMenu.TryGetComponent(out ParentConstraint menuConstraint);
        if (menuConstraint != null)
        {
            menuConstraint.constraintActive = true;
        }
        _volumeColorAdjustments.saturation.value = 0;
        player.EnableGameHands();
    }

    public void ShowStartMenu()
    {
        mainMenu.gameObject.SetActive(true);
        mainMenu.TryGetComponent(out ParentConstraint menuConstraint);
        
        // Stop the menu from moving around while it's open
        if (menuConstraint != null)
        {
            StartCoroutine(DisableMenuConstraint(menuConstraint));
        }
        
        player.EnableUIHands();
    }
 
    public void HideStartMenu()
    {
        mainMenu.gameObject.SetActive(false);
        mainMenu.TryGetComponent(out ParentConstraint menuConstraint);
        _volumeColorAdjustments.saturation.value = 0;
        player.EnableUIHands();
        
        if (menuConstraint != null)
        {
            menuConstraint.constraintActive = true;
        }
    }

    /// <summary>
    /// Disables the <paramref name="constraint"/> to stop it from following the player.
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

    public void StartGame()
    {
        HideStartMenu();
        Unpause();
        if (player.startingWeapon != null)
        {
            player.ForceEquipWeapon(player.startingWeapon);
            player.ToggleUIRays();
            player.EnableHands();
        }
        pauseButton.action.Enable();
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
}
