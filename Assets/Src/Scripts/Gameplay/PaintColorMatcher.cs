using System.Collections.Generic;
using Gameplay;
using UnityEngine;

namespace Src.Scripts.Gameplay
{
    /// <summary>
    /// This is used to set the paint color of desired objects to match their team's paint
    /// or the color of paint in the environment.
    /// </summary>
    public class PaintColorMatcher : MonoBehaviour
    {
        [Tooltip("The color of these will match the environment paint color")]
        public List<PaintColorManager> matchEnvironmentColor; 
        [Tooltip("The color of these will match the team paint color")]
        public List<PaintColorManager> matchTeamColor;

        public int EnvironmentChannel { get; private set; }

        private void Start()
        {
            UpdateTeamColor();
        }

        public void UpdateEnvironmentColor(int newChannel)
        {
            EnvironmentChannel = newChannel; 
            foreach (var manager in matchEnvironmentColor)
            {
                manager.UpdateColorChannel(EnvironmentChannel);
            }
        }

        public void UpdateTeamColor()
        {
            if (TryGetComponent(out TeamMember member))
            {
                foreach (var manager in matchTeamColor)
                {
                    manager.UpdateColorChannel(member.teamChannel);
                }
            }
        }
    }
}