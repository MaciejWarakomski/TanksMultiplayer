﻿using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Networking.Client
{
    public class NetworkClient
    {
        private readonly NetworkManager _networkManager;

        private const string MenuSceneName = "Menu";
        
        public NetworkClient(NetworkManager networkManager)
        {
            _networkManager = networkManager;

            networkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            if (clientId != 0 && clientId != _networkManager.LocalClientId) return;

            if (SceneManager.GetActiveScene().name != MenuSceneName)
            {
                SceneManager.LoadScene(MenuSceneName);
            }

            if (_networkManager.IsConnectedClient)
            {
                _networkManager.Shutdown();
            }
        }
    }
}