using System;
using UnityEngine;
using Unity.Netcode;
using Networking.Shared;
using System.Threading.Tasks;
using Unity.Services.Matchmaker.Models;

namespace Networking.Server
{
    public class ServerGameManager : IDisposable
    {
        public NetworkServer NetworkServer { get; private set; }
        
        private MatchplayBackfiller _backfiller;
        
        private readonly MultiplayAllocationService _multiplayAllocationService;
        private readonly NetworkObject _playerPrefab;
        
        private int _queryPort;
        
        private readonly string _serverIP;
        private readonly int _serverPort;
        
        public ServerGameManager(string serverIP, int serverPort, int queryPort, 
            NetworkManager manager, NetworkObject playerPrefab)
        {
            _serverIP = serverIP;
            _serverPort = serverPort;
            _queryPort = queryPort;
            
            NetworkServer = new NetworkServer(manager, playerPrefab);
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
                    NetworkServer.OnUserJoined += UserJoined;
                    NetworkServer.OnUserLeft += UserLeft;
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
            
            if (!NetworkServer.OpenConnection(_serverIP, _serverPort))
            {
                Debug.LogWarning("NetworkServer did not start as expected.");
                return;
            }
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
            NetworkServer.OnUserJoined -= UserJoined;
            NetworkServer.OnUserLeft -= UserLeft;
            
            _backfiller?.Dispose();
            _multiplayAllocationService?.Dispose();
            NetworkServer?.Dispose();
        }
    }
}