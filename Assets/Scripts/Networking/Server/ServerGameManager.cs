using System;
using UnityEngine;
using Unity.Netcode;
using Networking.Shared;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.Services.Matchmaker.Models;

namespace Networking.Server
{
    public class ServerGameManager : IDisposable
    {
        private NetworkServer _networkServer;
        private MultiplayAllocationService _multiplayAllocationService;
        private MatchplayBackfiller _backfiller;
        
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

            try
            {
                var matchmakerPayload = await GetMatchmakerPayload();

                if (matchmakerPayload != null)
                {
                    await StartBackfill(matchmakerPayload);
                    _networkServer.OnUserJoined += UserJoined;
                    _networkServer.OnUserLeft += UserLeft;
                }
                else
                {
                    Debug.LogWarning("Matchmaker payload timed out");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
            
            if (!_networkServer.OpenConnection(_serverIP, _serverPort))
            {
                Debug.LogWarning("NetworkServer did not start as expected.");
                return;
            }
            
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }

        private async Task<MatchmakingResults> GetMatchmakerPayload()
        {
            var matchmakerPayloadTask = _multiplayAllocationService.SubscribeAndAwaitMatchmakerAllocation();

            if (await Task.WhenAny(matchmakerPayloadTask, Task.Delay(20000)) == matchmakerPayloadTask)
            {
                return matchmakerPayloadTask.Result;
            }

            return null;
        }

        private async Task StartBackfill(MatchmakingResults payload)
        {
            _backfiller = new MatchplayBackfiller($"{_serverIP}:{_serverPort}", 
                payload.QueueName, payload.MatchProperties, 20);

            if (_backfiller.NeedsPlayers())
            {
                await _backfiller.BeginBackfilling();
            }
        }

        private void UserJoined(UserData user)
        {
            _backfiller.AddPlayerToMatch(user);
            _multiplayAllocationService.AddPlayer();
            if (!_backfiller.NeedsPlayers() && _backfiller.IsBackfilling)
            {
                _ = _backfiller.StopBackfill();
            }
        }
        
        private void UserLeft(UserData user)
        {
            var playerCount = _backfiller.RemovePlayerFromMatch(user.userAuthId);
            _multiplayAllocationService.RemovePlayer();
            
            if (playerCount <= 0)
            {
                CloseServer();
                return;
            }

            if (_backfiller.NeedsPlayers() && !_backfiller.IsBackfilling)
            {
                _ = _backfiller.BeginBackfilling();
            }
        }

        private async void CloseServer()
        {
            await _backfiller.StopBackfill();
            Dispose();
            Application.Quit();
        }
        
        public void Dispose()
        {
            _networkServer.OnUserJoined -= UserJoined;
            _networkServer.OnUserLeft -= UserLeft;
            
            _backfiller?.Dispose();
            _multiplayAllocationService?.Dispose();
            _networkServer?.Dispose();
        }
    }
}