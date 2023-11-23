using UnityEngine;

namespace Src.Scripts.Utility
{
    /// <summary>
    /// Stores the position the player should be placed on scene load.
    /// </summary>
    public class SpawnPoint : Singleton<SpawnPoint>
    {
        public Vector3 initialPosition;
        public bool checkpointReached;
        
        public override void Awake()
        {
            base.Awake();
            if (initialPosition == Vector3.zero)
            {
                initialPosition = transform.position;
            }
            DontDestroyOnLoad(gameObject);
        }

        public void Reset()
        {
            transform.position = initialPosition;
            checkpointReached = false;
        }
    }
}
