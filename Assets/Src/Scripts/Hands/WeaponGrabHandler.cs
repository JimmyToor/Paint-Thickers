using Src.Scripts.Gameplay;
using Src.Scripts.Weapons;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Src.Scripts.Hands
{
    public class WeaponGrabHandler : MonoBehaviour
    {
        public XRDirectInteractor interactor;
        public Player player;
        
        // Start is called before the first frame update
        void Start()
        {
            interactor.selectEntered.AddListener(SetPlayerWeapon);
            interactor.selectExited.AddListener(UnequipPlayerWeapon);
        }

        private void SetPlayerWeapon(SelectEnterEventArgs args)
        {
            if (args.interactable.TryGetComponent(out Weapon weapon))
            {
                player.SetWeapon(weapon);
            }
        }

        private void UnequipPlayerWeapon(SelectExitEventArgs args)
        {
            if (args.interactable.TryGetComponent(out Weapon weapon))
            {
                player.TryUnequipWeapon(weapon);
            }
        }
    }
}
