using UnityEngine;

namespace Src.Scripts.Utility
{
    public class Singleton<T> : MonoBehaviour where T: MonoBehaviour{
    
        public static bool Verbose = false;
        public static bool KeepAlive = false;

        private static T _instance = null;
        public static T Instance {
            get {
                if (_instance != null) return _instance;
                
                _instance = FindObjectOfType<T>();
                if (_instance != null) return _instance;
                if (Verbose)
                    Debug.Log("No instance of " + typeof(T) + " exists so one will be created.");

                var singletonObj = new GameObject
                {
                    name = typeof(T).ToString()
                };

                _instance = singletonObj.AddComponent<T>();
                return _instance;
            }
        }

        public static bool IsInstanceAlive => _instance != null;

        public virtual void Awake(){
            if (_instance != null && _instance != this){
                if(Verbose)
                    Debug.Log("SingleAccessPoint, Destroy duplicate instance " + name + " of " + Instance.name, this);
                Destroy(gameObject);
                return;
            }

            _instance = GetComponent<T>();
        
            if(KeepAlive){
                DontDestroyOnLoad(gameObject);
            }
        
            if (_instance == null){
                if(Verbose)
                    Debug.LogError("SingleAccessPoint<" + typeof(T).Name + "> Instance null in Awake");
                return;
            }

            if(Verbose)
                Debug.Log("SingleAccessPoint instance found " + Instance.GetType().Name);
        }

    }
}