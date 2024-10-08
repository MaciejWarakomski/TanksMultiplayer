﻿using UI;
using System;
using UnityEngine;
using Unity.Netcode;
using Networking.Server;
using Networking.Shared;
using System.Collections;
using Unity.Services.Relay;
using Unity.Services.Lobbies;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using Unity.Networking.Transport.Relay;

namespace Networking.Host
{
    public class HostGameManager : IDisposable
    {
        public NetworkServer NetworkServer { get; private set; }

        private Allocation _allocation;

        private readonly NetworkObject _playerPrefab;
        
        private string _joinCode;
        private string _lobbyId;

        private const string GameSceneName = "Game";
        private const int MaxConnections = 20;

        public HostGameManager(NetworkObject playerPrefab)
        {
            _playerPrefab = playerPrefab;
        }
        
        public async Task StartHostAsync()
        {
            try
            {
                _allocation = await Relay.Instance.CreateAllocationAsync(MaxConnections);
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
                return;
            }

            try
            {
                _joinCode = await Relay.Instance.GetJoinCodeAsync(_allocation.AllocationId);
                Debug.Log(_joinCode);
            }
            catch (Exception exception)
            {
                Debug.Log(exception);
                return;
            }

            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            var relayServerData = new RelayServerData(_allocation, "dtls");
            unityTransport.SetRelayServerData(relayServerData);

            try
            {
                var lobbyOptions = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>()
                    {
                        {
                            "JoinCode", new DataObject(
                                visibility: DataObject.VisibilityOptions.Member,
                                value: _joinCode
                            )
                        }
                    }
                };

                var playerName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Unknown");
                var lobby = await Lobbies.Instance.CreateLobbyAsync(
                    $"{playerName}'s Lobby", MaxConnections, lobbyOptions);

                _lobbyId = lobby.Id;
                HostSingleton.Instance.StartCoroutine(HeartbeatLobby(15));
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
                return;
            }

            NetworkServer = new NetworkServer(NetworkManager.Singleton, _playerPrefab);

            var userData = new UserData
            {
                userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
                userAuthId = AuthenticationService.Instance.PlayerId
            };
            var payload = JsonUtility.ToJson(userData);
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;

            NetworkManager.Singleton.StartHost();

            NetworkServer.OnClientLeft += HandleClientLeft;

            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }

        private IEnumerator HeartbeatLobby(float waitTimeSeconds)
        {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true)
            {
                Lobbies.Instance.SendHeartbeatPingAsync(_lobbyId);
                yield return delay;
            }
        }

        public void Dispose()
        {
            Shutdown();
        }

        public async void Shutdown()
        {
            if (string.IsNullOrEmpty(_lobbyId)) return;

            HostSingleton.Instance.StopCoroutine(nameof(HeartbeatLobby));

            try
            {
                await Lobbies.Instance.DeleteLobbyAsync(_lobbyId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }

            _lobbyId = string.Empty;

            NetworkServer.OnClientLeft -= HandleClientLeft;

            NetworkServer?.Dispose();
        }

        private async void HandleClientLeft(string authId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(_lobbyId, authId);
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
    }
}