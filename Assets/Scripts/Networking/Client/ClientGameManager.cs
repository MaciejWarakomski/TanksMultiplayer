﻿using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Relay;
using System.Threading.Tasks;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

namespace Networking.Client
{
    public class ClientGameManager
    {
        private JoinAllocation _allocation;
        
        private const string MenuSceneName = "Menu";
        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();
            var authState = await AuthenticationWrapper.DoAuth();

            return authState == AuthState.Authenticated;
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        public async Task StartClientAsync(string joinCode)
        {
            try
            {
                _allocation = await Relay.Instance.JoinAllocationAsync(joinCode);
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
                return;
            }

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = new RelayServerData(_allocation, "dtls");
            unityTransport.SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
    }
}