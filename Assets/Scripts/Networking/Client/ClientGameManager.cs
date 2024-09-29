using UI;
using System;
using UnityEngine;
using Unity.Netcode;
using Networking.Shared;
using Unity.Services.Core;
using Unity.Services.Relay;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Networking.Transport.Relay;

namespace Networking.Client
{
    public class ClientGameManager : IDisposable
    {
        private JoinAllocation _allocation;
        private NetworkClient _networkClient;
        private MatchplayMatchmaker _matchplayMatchmaker;
        private UserData _userData;
        
        private const string MenuSceneName = "Menu";
        
        public async Task<bool> InitAsync()
        {
            await UnityServices.InitializeAsync();

            _networkClient = new NetworkClient(NetworkManager.Singleton);
            _matchplayMatchmaker = new MatchplayMatchmaker();
            var authState = await AuthenticationWrapper.DoAuth();

            if (authState == AuthState.Authenticated)
            {
                _userData = new UserData
                {
                    userName = PlayerPrefs.GetString(NameSelector.PlayerNameKey, "Missing Name"),
                    userAuthId = AuthenticationService.Instance.PlayerId
                };
                return true;
            }

            return false;
        }

        public void GoToMenu()
        {
            SceneManager.LoadScene(MenuSceneName);
        }

        public void StartClient(string ip, int port)
        {
            var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            unityTransport.SetConnectionData(ip, (ushort)port);
            ConnectClient();
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

            ConnectClient();
        }

        private void ConnectClient()
        {
            var payload = JsonUtility.ToJson(_userData);
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
            
            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
            NetworkManager.Singleton.StartClient();
        }

        public async void MatchmakeAsync(Action<MatchmakerPollingResult> onMachmakeResponse)
        {
            if (_matchplayMatchmaker.IsMatchmaking) return;
            
            var matchResult = await GetMatchAsync();
            onMachmakeResponse?.Invoke(matchResult);
        }
        
        private async Task<MatchmakerPollingResult> GetMatchAsync()
        {
            var matchmakingResult = await _matchplayMatchmaker.Matchmake(_userData);

            if (matchmakingResult.result == MatchmakerPollingResult.Success)
            {
                StartClient(matchmakingResult.ip, matchmakingResult.port);
            }
            
            return matchmakingResult.result;
        }

        public async Task CancelMatchmaking()
        {
            await _matchplayMatchmaker.CancelMatchmaking();
        }
        
        public void Disconnect()
        {
            _networkClient.Disconnect();
        }
        
        public void Dispose()
        {
            _networkClient?.Dispose();
        }
    }
}