using System;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Networking.Server
{
    public class ServerGameManager : IDisposable
    {
        private NetworkServer _networkServer;
        private MultiplayAllocationService _multiplayAllocationService;
        
        private string _serverIP;
        private int _serverPort;
        private int _queryPort;
        
        private const string GameSceneName = "Game";
        
        public ServerGameManager(string serverIP, int serverPort, int queryPort, NetworkManager manager)
        {
            _serverIP = serverIP;
            _serverPort = serverPort;
            _queryPort = queryPort;
            _networkServer = new NetworkServer(manager);
            _multiplayAllocationService = new MultiplayAllocationService();
        }
        
        public async Task StartGameServerAsync()
        {
            await _multiplayAllocationService.BeginServerCheck();
            if (!_networkServer.OpenConnection(_serverIP, _serverPort))
            {
                Debug.LogWarning("NetworkServer did not start as expected.");
                return;
            }
            
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
        
        public void Dispose()
        {
            _multiplayAllocationService?.Dispose();
            _networkServer?.Dispose();
        }
    }
}