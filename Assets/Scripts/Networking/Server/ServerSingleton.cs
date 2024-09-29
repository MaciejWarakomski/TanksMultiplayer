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
                return _instance ? _instance : null;
            }
        }

        public ServerGameManager GameManager { get; private set; }

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public async Task CreateServer(NetworkObject playerPrefab)
        {
            await UnityServices.InitializeAsync();
            GameManager = new ServerGameManager(
                ApplicationData.IP(),
                ApplicationData.Port(),
                ApplicationData.QPort(),
                NetworkManager.Singleton,
                playerPrefab
            );
        }

        private void OnDestroy()
        {
            GameManager?.Dispose();
        }
    }
}