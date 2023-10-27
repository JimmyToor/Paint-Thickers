using UnityEngine;

namespace Src.Scripts.Utility
{
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
