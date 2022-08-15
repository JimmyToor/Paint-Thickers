using ScriptableObjects;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public Player player;
    public GameObject gameOverUI;
    public TeamColorScriptableObject teamColors;
    
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

    public Color GetTeamColor(int channel)
    {
        return teamColors.teamColors[channel];
    }
}
