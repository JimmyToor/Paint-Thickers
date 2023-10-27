using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;

namespace Src.Scripts.UI
{
    public class GameMenu : MonoBehaviour
    {
        private List<IConstraint> _menuConstraints;
        public UnityEvent onShow;
        public UnityEvent onHide;

        private void Awake()
        {
            _menuConstraints = new List<IConstraint>();
            if (_menuConstraints.Count == 0)
            {
                _menuConstraints = GetComponents<IConstraint>()?.ToList();
            }
        }

        public void ShowMenu()
        {
            transform.localScale = Vector3.one;
        
            // Stop the menu from moving around while it's open
            DisableConstraints();
            onShow.Invoke();
        }
    
        public void HideMenu()
        {
            transform.localScale = Vector3.zero;
            
            // Allow the menu to move around while it's hidden
            EnableConstraints();
            onHide.Invoke();
        }

        public void ToggleMenu()
        {
            if (transform.localScale == Vector3.zero)
            {
                ShowMenu();
            }
            else
            {
                HideMenu();
            }
        }

        private void EnableConstraints()
        {
            foreach (var constraint in _menuConstraints)
            {
                constraint.constraintActive = true;
            }
        }
        
        private void DisableConstraints()
        {
            foreach (var constraint in _menuConstraints)
            {
                constraint.constraintActive = false;
            }
        }
    }
}
