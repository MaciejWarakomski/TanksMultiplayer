using UI;
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
using Unity.Services.Lobbies.Models;
using Unity.Networking.Transport.Relay;

namespace Networking.Host
{
    public class HostGameManager
    {
        private Allocation _allocation;
        private NetworkServer _networkServer;

        private string _joinCode;
        private string _lobbyId;

        private const string GameSceneName = "Game";
        private const int MaxConnections = 20;

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

            _networkServer = new NetworkServer(NetworkManager.Singleton);
            
            var userData = new UserData
            {
                userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name")
            };
            var payload = JsonUtility.ToJson(userData);
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            
            NetworkManager.Singleton.StartHost();
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
    }
}