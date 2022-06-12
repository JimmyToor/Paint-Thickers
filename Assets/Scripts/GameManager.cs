using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Player player;
    public GameObject gameOverUI;
    public TeamColorScriptableObject teamColors;
    
    private Color[] TeamColors;

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
    }

    public void PlayerDeath()
    {
        player.DisableOverlayUI();
        player.DisableWeapon();
        player.ToggleUIRays();
        ShowGameOverUI();
    }

    private void ShowGameOverUI()
    {
        gameOverUI.SetActive(true);
        gameOverUI.GetComponent<ParentConstraint>().constraintActive = false;
    }
    
    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void UpdateTeamColors()
    {
        if (TeamColors.Length != 4)
        {
            TeamColors = new Color[4];
        }
        
        TeamColors[0] = teamColors.teamColor1;
        TeamColors[1] = teamColors.teamColor2;
        TeamColors[2] = teamColors.teamColor3;
        TeamColors[3] = teamColors.teamColor4;
    }

    public Color GetTeamColor(int channel)
    {
        return TeamColors[channel];
    }
}
