using UnityEngine;

namespace Src.Scripts.Utility
{
    public class Singleton<T> : MonoBehaviour where T: MonoBehaviour{
    
        public static bool Verbose = false;
        public static bool KeepAlive = true;

        private static T _instance = null;
        public static T Instance {
            get { 
                if(_instance == null){
                    _instance = GameObject.FindObjectOfType<T>();
                    if(_instance == null){
                        var singletonObj = new GameObject();
                        singletonObj.name = typeof(T).ToString();
                        _instance = singletonObj.AddComponent<T>();
                    }
                }
                return _instance;
            }
        }

        static public bool IsInstanceAlive{
            get { return _instance != null; }
        }

        public virtual void Awake(){
            if (_instance != null){
                if(Verbose)
                    Debug.Log("SingleAccessPoint, Destroy duplicate instance " + name + " of " + Instance.name);
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