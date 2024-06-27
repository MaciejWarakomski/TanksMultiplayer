using UnityEngine;
using System.Threading.Tasks;

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

        private HostGameManager _gameManager;
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void CreateHost()
        {
            _gameManager = new HostGameManager();
        }
    }
}