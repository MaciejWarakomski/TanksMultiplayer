﻿using UnityEngine;
using Core.Player;
using Unity.Netcode;
using System.Collections;

namespace Core.Combat
{
    public class RespawnHandler : NetworkBehaviour
    {
        [SerializeField] private TankPlayer playerPrefab;
        [SerializeField] private float keptCoinPercentage = 50f;

        public override void OnNetworkSpawn()
        {
            if (!IsServer) return;

            var players = FindObjectsByType<TankPlayer>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                HandlePlayerSpawned(player);
            }
            
            TankPlayer.OnPlayerSpawned += HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned += HandlePlayerDespawned;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsServer) return;
            
            TankPlayer.OnPlayerSpawned -= HandlePlayerSpawned;
            TankPlayer.OnPlayerDespawned -= HandlePlayerDespawned;
        }
        
        private void HandlePlayerSpawned(TankPlayer player)
        {
            player.Health.OnDie += _ => HandlePlayerDie(player);
        }
        
        private void HandlePlayerDespawned(TankPlayer player)
        {
            player.Health.OnDie -= _ => HandlePlayerDie(player);
        }

        private void HandlePlayerDie(TankPlayer player)
        {
            var keptCoins = (int)(player.Wallet.TotalCoins.Value * (keptCoinPercentage / 100));
            
            Destroy(player.gameObject);

            StartCoroutine(RespawnPlayer(player.OwnerClientId, keptCoins));
        }

        private IEnumerator RespawnPlayer(ulong ownerClientId, int keptCoins)
        {
            yield return null;
            
            var playerInstance = Instantiate(
                playerPrefab, SpawnPoint.GetRandomSpawnPos(), Quaternion.identity);
            playerInstance.NetworkObject.SpawnAsPlayerObject(ownerClientId);
            playerInstance.Wallet.TotalCoins.Value += keptCoins;
        }
    }
}