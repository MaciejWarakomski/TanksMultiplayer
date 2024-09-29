using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using System.Threading.Tasks;

namespace Networking.Server
{
    public class ServerSingleton : MonoBehaviour
    {
        private static ServerSingleton _instance;

        public static ServerSingleton Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = FindObjectOfType<ServerSingleton>();
                if (_instance) return _instance;

                Debug.LogError("No ServerSingleton in the scene!");
                return null;
            }
        }

        public ServerGameManager GameManager { get; private set; }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task CreateServer()
        {
            await UnityServices.InitializeAsync();
            GameManager = new ServerGameManager(
                ApplicationData.IP(),
                ApplicationData.Port(),
                ApplicationData.QPort(),
                NetworkManager.Singleton
            );
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}