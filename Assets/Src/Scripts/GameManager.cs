using System.Collections;
using DG.Tweening;
using Src.Scripts.Gameplay;
using Src.Scripts.ScriptableObjects;
using Src.Scripts.Utility;
using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Src.Scripts
{
    public class GameManager : Singleton<GameManager>
    {
        public Player player;
        public TeamColorScriptableObject teamColorData;
        [Tooltip("Invoked on first level load.")]
        public UnityEvent onGameInit;
        [Tooltip("Invoked when gameplay begins.")]
        public UnityEvent onGameStart;

        public GameObject _lastWeapon;
        
        private static readonly int PaintColor1 = Shader.PropertyToID("_PaintColor1");
        private static readonly int PaintColor2 = Shader.PropertyToID("_PaintColor2");
        private static readonly int PaintColor3 = Shader.PropertyToID("_PaintColor3");
        private static readonly int PaintColor4 = Shader.PropertyToID("_PaintColor4");

        private void Start()
        {
            Initialize();
        }

        public override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        private void Initialize()
        {
            onGameInit.Invoke();
            SetShaderColors();
        }

        public IEnumerator RestartLevel()
        {
            Time.timeScale = 1;
            SpawnPoint.Instance.Reset();
            DOTween.KillAll();
            var asyncLoadLevel = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            while (!asyncLoadLevel.isDone)
            {
                Debug.Log("Loading the Scene");
                yield return null;
            }

            onGameInit.Invoke();
        }

        /// <summary>
        /// Sets the color of each paint channel based on the team color data
        /// </summary>
        private void SetShaderColors()
        {
            Shader.SetGlobalColor(PaintColor1,teamColorData.GetTeamColor(0));
            Shader.SetGlobalColor(PaintColor2,teamColorData.GetTeamColor(1));
            Shader.SetGlobalColor(PaintColor3,teamColorData.GetTeamColor(2));
            Shader.SetGlobalColor(PaintColor4,teamColorData.GetTeamColor(3));
        }
        
        [ContextMenu("Start Game")]
        public void StartGame()
        {
            onGameStart.Invoke();
        }
        
        public static void Quit()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
            Application.Quit();
        }
        
        public void RetryLevel()
        {
            StartCoroutine(!SpawnPoint.Instance.checkpointReached ? RestartLevel() : RestartFromCheckpoint());
        }
        
        /// <summary>
        /// Reloads level and places player at last checkpoint reached with their weapon.
        /// Restarts level if no checkpoint reached.
        /// </summary>
        public IEnumerator RestartFromCheckpoint()
        {
            Time.timeScale = 1;
            DOTween.KillAll();
            
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }
            
            var asyncLoadLevel = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
            while (!asyncLoadLevel.isDone)
            {
                yield return null;
            }
            
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }
            
            onGameStart.Invoke();
            
            if (_lastWeapon != null)
            {
                GameObject weaponObject = Instantiate(_lastWeapon);
                weaponObject.TryGetComponent(out Weapon weapon);
                player.ForceEquipWeapon(weapon);
            }
        }

        public void NewCheckpoint(Checkpoint checkpoint)
        {
            SpawnPoint.Instance.transform.position = checkpoint.transform.position;
            SpawnPoint.Instance.checkpointReached = true;
            RememberWeapon();
        }

        private void RememberWeapon()
        {
            if (player == null)
            {
                player = FindObjectOfType<Player>();
            }
            if (player.weaponHandler.Weapon != null)
            {
                _lastWeapon = player.weaponHandler.Weapon.wepParams.weaponPrefab;
            }
        }
    }
}
