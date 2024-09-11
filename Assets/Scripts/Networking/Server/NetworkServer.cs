using UnityEngine;
using Unity.Netcode;
using Networking.Shared;
using System.Collections.Generic;

namespace Networking.Server
{
    public class NetworkServer
    {
        private readonly NetworkManager _networkManager;
        private readonly Dictionary<ulong, string> _clientIdToAuthId = new();
        private readonly Dictionary<string, UserData> _authIdToUserData = new();
        
        public NetworkServer(NetworkManager networkManager)
        {
            _networkManager = networkManager;

            networkManager.ConnectionApprovalCallback += ApprovalCheck;
            networkManager.OnServerStarted += OnNetworkReady;
        }

        private void ApprovalCheck(
            NetworkManager.ConnectionApprovalRequest request, 
            NetworkManager.ConnectionApprovalResponse response)
        {
            var payload = System.Text.Encoding.UTF8.GetString(request.Payload);
            var userData = JsonUtility.FromJson<UserData>(payload);

            _clientIdToAuthId[request.ClientNetworkId] = userData.userAuthId;
            _authIdToUserData[userData.userAuthId] = userData;

            response.Approved = true;
            response.CreatePlayerObject = true;
        }
        
        private void OnNetworkReady()
        {
            _networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (_clientIdToAuthId.Remove(clientId, out var authId))
            {
                _authIdToUserData.Remove(authId);
            }
            
        }
    }
}