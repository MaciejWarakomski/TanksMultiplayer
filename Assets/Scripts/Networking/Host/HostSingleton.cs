using UnityEngine;

namespace Networking.Host
{
    public class HostSingleton : MonoBehaviour
    {
        private static HostSingleton _instance;

        public static HostSingleton Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = FindObjectOfType<HostSingleton>();
                if (_instance) return _instance;
                
                Debug.LogError("No HostSingleton in the scene!");
                return null;
            }
        }

        public HostGameManager GameManager { get; private set; }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void CreateHost()
        {
            GameManager = new HostGameManager();
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}