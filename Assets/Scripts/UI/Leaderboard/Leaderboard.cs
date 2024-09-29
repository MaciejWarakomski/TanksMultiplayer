using Core.Player;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

namespace UI.Leaderboard
{
    public class Leaderboard : NetworkBehaviour
    {
        [SerializeField] private Transform leaderboardEntityHolder;
        [SerializeField] private LeaderboardEntityDisplay leaderboardEntityPrefab;

        [SerializeField] private int entitiesToDisplay = 8;
        
        private NetworkList<LeaderboardEntityState> _leaderboardEntities;
        private readonly List<LeaderboardEntityDisplay> _entityDisplays = new();

        private void Awake()
        {
            _leaderboardEntities = new NetworkList<LeaderboardEntityState>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                _leaderboardEntities.OnListChanged += HandleLeaderboardEntitiesChanged;
                foreach (var entity in _leaderboardEntities)
                {
                    HandleLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                    {
                        Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                        Value = entity
                    });
                }
            }
            if (IsServer)
            {
                var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
                foreach (var player in players)
                {
                    HandlePlayerSpawned(player);
                }
            
                TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsClient)
            {
                _leaderboardEntities.OnListChanged -= HandleLeaderboardEntitiesChanged;
            }
            if (IsServer)
            {
                TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
                TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
            }
        }
        
        private void HandleLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
        {
            if (!gameObject.scene.isLoaded) return;
            
            switch (changeEvent.Type)
            {
                case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                    if (_entityDisplays.All(x => x.ClientId != changeEvent.Value.ClientId))
                    {
                        var leaderboardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntityHolder);
                        leaderboardEntity.Initialize(
                            changeEvent.Value.ClientId, 
                            changeEvent.Value.PlayerName, 
                            changeEvent.Value.Coins);
                        _entityDisplays.Add(leaderboardEntity);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                    var displayToRemove = _entityDisplays.FirstOrDefault(
                        x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayToRemove != null)
                    {
                        displayToRemove.transform.SetParent(null);
                        Destroy(displayToRemove.gameObject);
                        _entityDisplays.Remove(displayToRemove);
                    }
                    break;
                case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                    var displayToUpdate = _entityDisplays.FirstOrDefault(
                        x => x.ClientId == changeEvent.Value.ClientId);
                    if (displayToUpdate != null)
                    {
                        displayToUpdate.UpdateCoins(changeEvent.Value.Coins);
                    }
                    break;
            }

            _entityDisplays.Sort((x, y) => y.Coins.CompareTo(x.Coins));
            for (var i = 0; i < _entityDisplays.Count; i++)
            {
                _entityDisplays[i].transform.SetSiblingIndex(i);
                _entityDisplays[i].UpdateText();
                _entityDisplays[i].gameObject.SetActive(i <= entitiesToDisplay - 1);
            }

            var myDisplay = _entityDisplays.FirstOrDefault(
                x => x.ClientId == NetworkManager.Singleton.LocalClientId);
            if (myDisplay != null && myDisplay.transform.GetSiblingIndex() >= entitiesToDisplay)
            {
                leaderboardEntityHolder.GetChild(entitiesToDisplay - 1).gameObject.SetActive(false);
                myDisplay.gameObject.SetActive(true);
            }
        }

        private void HandlePlayerSpawned(TankPlayer player)
        {
            _leaderboardEntities.Add(new LeaderboardEntityState
            {
                ClientId = player.OwnerClientId,
                PlayerName = player.playerName.Value,
                Coins = 0
            });

            player.Wallet.TotalCoins.OnValueChanged += (_, newCoins) => 
                HandleCoinsChanged(player.OwnerClientId, newCoins);
        }
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            if (_leaderboardEntities == null) return;
            
            foreach (var entity in _leaderboardEntities)
            {
                if (entity.ClientId != player.OwnerClientId) continue;
                _leaderboardEntities.Remove(entity);
                break;
            }
            
            player.Wallet.TotalCoins.OnValueChanged -= (_, newCoins) => 
                HandleCoinsChanged(player.OwnerClientId, newCoins);
        }
        
        private void HandleCoinsChanged(ulong clientId, int newCoins) 
        {
            for (var i = 0; i < _leaderboardEntities.Count; i++)
            {
                if (_leaderboardEntities[i].ClientId != clientId) continue;

                _leaderboardEntities[i] = new LeaderboardEntityState
                {
                    ClientId = _leaderboardEntities[i].ClientId,
                    PlayerName = _leaderboardEntities[i].PlayerName,
                    Coins = newCoins
                };
                return;
            }
        }
    }
}