using System.Collections.Generic;
using UnityEngine;

namespace Gameplay
{
    public class PaintColorHandler : MonoBehaviour
    {
        public List<PaintHandler> matchEnvironmentColor;
        public List<PaintHandler> matchTeamColor;
        [HideInInspector]
        public int environmentChannel;

        
        private void Start()
        {
            InitColors();
        }

        public void UpdateEnvironmentColor(int newChannel)
        {
            environmentChannel = newChannel; 
            foreach (var handler in matchEnvironmentColor)
            {
                handler.UpdateColorChannel(environmentChannel);
            }
        }

        private void InitColors()
        {
            if (TryGetComponent(out TeamMember member))
            {
                foreach (var handler in matchTeamColor)
                {
                    handler.UpdateColorChannel(member.teamChannel);
                }
            }
        }
    }
}