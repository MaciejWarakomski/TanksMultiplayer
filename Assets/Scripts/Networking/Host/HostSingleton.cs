using UnityEngine;
using Unity.Netcode;

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
                return _instance ? _instance : null;
            }
        }

        public HostGameManager GameManager { get; private set; }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void CreateHost(NetworkObject playerPrefab)
        {
            GameManager = new HostGameManager(playerPrefab);
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}