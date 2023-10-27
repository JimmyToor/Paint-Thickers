using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Src.Scripts.Utility
{
    public class PauseHandler : MonoBehaviour
    {
        public InputActionProperty pauseButton;
        public UnityEvent onPauseButton;
        public UnityEvent onPause;
        public UnityEvent onResume;

        private void OnEnable()
        {
            pauseButton.action.performed += OnPauseButtonPressed;
        }
        
        private void OnDisable()
        {
            pauseButton.action.performed -= OnPauseButtonPressed;
        }
        
        private void OnPauseButtonPressed(InputAction.CallbackContext callbackContext = new InputAction.CallbackContext())
        {
            onPauseButton?.Invoke();
        }
        
        public void Pause()
        {
            onPause?.Invoke();
            Time.timeScale = 0f;
        }

        public void Unpause()
        { 
            Time.timeScale = 1f;
            onResume?.Invoke();
        }
        
        public void EnablePauseButton()
        {
            pauseButton.action.Enable();
        } 
    
        public void DisablePauseButton()
        {
            pauseButton.action.Disable();
        }
        
        [ContextMenu("Toggle Pause")]
        public void TogglePause()
        {
            if (Time.timeScale == 0f)
            {
                Unpause();
            }
            else
            {
                Pause();
            }
        }
    }
}