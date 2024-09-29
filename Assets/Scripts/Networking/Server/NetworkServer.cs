using Core;
using System;
using UnityEngine;
using Unity.Netcode;
using Networking.Shared;
using System.Threading.Tasks;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using Unity.Netcode.Transports.UTP;

namespace Networking.Server
{
    public class NetworkServer : IDisposable
    {
        private readonly NetworkManager _networkManager;
        private readonly NetworkObject _playerPrefab;
        private readonly Dictionary<ulong, string> _clientIdToAuthId = new();
        private readonly Dictionary<string, UserData> _authIdToUserData = new();

        public Action<UserData> OnUserJoined;
        public Action<UserData> OnUserLeft;
        public Action<string> OnClientLeft;
        
        public NetworkServer(NetworkManager networkManager, NetworkObject playerPrefab)
        {
            _networkManager = networkManager;
            _playerPrefab = playerPrefab;

            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += OnNetworkReady;
        }

        public bool OpenConnection(string ip, int port)
        {
            var unityTransport = _networkManager.gameObject.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(ip, (ushort)port);
            return _networkManager.StartServer();
        }
        
        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request, 
            NetworkManager.ConnectionApprovalResponse response)
        {
            var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            var userData = JsonUtility.FromJson<UserData>(payload);

            _clientIdToAuthId[request.ClientNetworkId] = userData.userAuthId;
            _authIdToUserData[userData.userAuthId] = userData;
            OnUserJoined?.Invoke(userData);

            _ = SpawnPlayerDelayed(request.ClientNetworkId);
            
            response.Approved = true;
            response.CreatePlayerObject = false;
        }

        private async Task SpawnPlayerDelayed(ulong clientId)
        {
            await Task.Delay(1000);
            
            var playerInstance = 
                Object.Instantiate(_playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
            
            playerInstance.SpawnAsPlayerObject(clientId);
        }
        
        private void OnNetworkReady()
        {
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_clientIdToAuthId.Remove(clientId, out var authId))
            {
                OnUserLeft?.Invoke(_authIdToUserData[authId]);
                _authIdToUserData.Remove(authId);
                OnClientLeft?.Invoke(authId);
            }
        }

        public UserData GetUserDataByClientId(ulong clientId)
        {
            if (_clientIdToAuthId.TryGetValue(clientId, out var authId))
            {
                if (_authIdToUserData.TryGetValue(authId, out var userData))
                {
                    return userData;
                }
            }
            return null;
        }
        
        public void Dispose()
        {
            if (_networkManager == null) return;
            _networkManager.ConnectionApprovalCallback -= ApprovalCheck;
            _networkManager.OnClientDisconnectCallback -= OnClientDisconnect;
            _networkManager.OnServerStarted -= OnNetworkReady;

            if (_networkManager.IsListening)
            {
                _networkManager.Shutdown();
            }
        }
    }
}